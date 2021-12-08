using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using NetSPF.Mechanisms;

namespace NetSPF
{
    public static class SpfResolver
    {
        /// <summary>
        /// Fetches SPF records, parses them, and
        /// evaluates them to determine whether a particular host is or is not
        /// permitted to send mail with a given identity.
        /// </summary>
        /// <param name="address">the IP address of the SMTP client that is emitting
        /// the mail, either IPv4 or IPv6.</param>
        /// <param name="domainName">The domain that provides the sought-after authorization
        /// information; initially, the domain portion of the
        /// "MAIL FROM" or "HELO" identity.</param>
        /// <param name="sender">the "MAIL FROM" or "HELO" identity.</param>
        /// <param name="heloDomain">Domain as presented by the client in the HELO or EHLO command.</param>
        /// <param name="hostDomain">Domain of the current host, performing SPF authentication.</param>
        /// <param name="dnsHost">The DNS host to query</param>
        /// <param name="spfExpressions">SPF Expressions that can be used, in case a domain lacks SPF records in the DNS.</param>
        /// <returns>Result of SPF evaluation, together with an optional explanation string,
        /// if one exists, and if the result indicates a failure.</returns>
        public static Task<KeyValuePair<SpfResult, string>> CheckHost(IPAddress address, string domainName,
            string sender, string heloDomain, string hostDomain, IPAddress dnsHost = null, params SpfExpression[] spfExpressions)
        {
            SpfStatement spfStatement = new SpfStatement(sender, domainName, address, heloDomain, hostDomain);
            return CheckHost(spfStatement, spfExpressions, dnsHost);
        }

        /// <summary>
        /// Fetches SPF records, parses them, and
        /// evaluates them to determine whether a particular host is or is not
        /// permitted to send mail with a given identity.
        /// </summary>
        /// <param name="spfStatement">Information about current query.</param>
        /// <param name="spfExpressions">SPF Expressions that can be used, in case a domain lacks SPF records in the DNS.</param>
        /// <param name="dnsHost">The DNS host to query</param>
        /// <returns>Result of SPF evaluation, together with an optional explanation string,
        /// if one exists, and if the result indicates a failure.</returns>
        internal static async Task<KeyValuePair<SpfResult, string>> CheckHost(SpfStatement spfStatement,
            SpfExpression[] spfExpressions, IPAddress dnsHost)
        {
            Explanation explanation = null;
            string[] spfStatementStrings = null;
            string s;

            try
            {
                string[] txt = await DnsResolver.LookupText(spfStatement.Domain, dnsHost);

                foreach (string row in txt)
                {
                    s = row.Trim();

                    if (s.Length > 1 && s[0] == '"' && s[s.Length - 1] == '"')
                        s = s.Substring(1, s.Length - 2);

                    if (!s.StartsWith("v=spf1"))
                        continue;

                    if (!(spfStatementStrings is null))
                        return new KeyValuePair<SpfResult, string>(SpfResult.PermanentError,
                            "Multiple SPF records found for " + spfStatement.Domain + ".");

                    spfStatementStrings = s.Substring(6).Trim().Split(Space, StringSplitOptions.RemoveEmptyEntries);
                }
            }
            catch (Exception)
            {
                spfStatementStrings = null;
            }

            if (spfStatementStrings is null)
            {
                if (!(spfExpressions is null))
                {
                    foreach (SpfExpression expression in spfExpressions)
                    {
                        if (expression.IsApplicable(spfStatement.Domain))
                        {
                            if (expression.Spf.StartsWith("v=spf1"))
                            {
                                spfStatementStrings = expression.Spf.Substring(6).Trim()
                                    .Split(Space, StringSplitOptions.RemoveEmptyEntries);
                                break;
                            }
                        }
                    }
                }

                if (spfStatementStrings is null)
                    return new KeyValuePair<SpfResult, string>(SpfResult.None,
                        "No SPF records found " + spfStatement.Domain + ".");
            }

            // Syntax evaluation first, §4.6

            int c = spfStatementStrings.Length;
            LinkedList<Mechanism> mechanisms = new LinkedList<Mechanism>();
            Redirect redirect = null;
            int i;

            try
            {
                for (i = 0; i < c; i++)
                {
                    SpfQualifier qualifier;

                    spfStatement.Reset(spfStatementStrings[i]);
                    spfStatement.SkipWhitespace();

                    switch (spfStatement.PeekNextCharacter())
                    {
                        case '+':
                            spfStatement.Position++;
                            qualifier = SpfQualifier.Pass;
                            break;

                        case '-':
                            spfStatement.Position++;
                            qualifier = SpfQualifier.Fail;
                            break;

                        case '~':
                            spfStatement.Position++;
                            qualifier = SpfQualifier.SoftFail;
                            break;

                        case '?':
                            spfStatement.Position++;
                            qualifier = SpfQualifier.Neutral;
                            break;

                        default:
                            qualifier = SpfQualifier.Pass;
                            break;
                    }

                    switch (spfStatement.NextLabel().ToLower())
                    {
                        case "all":
                            mechanisms.AddLast(new All(qualifier));
                            break;

                        case "include":
                            mechanisms.AddLast(new Include(spfStatement, qualifier, spfExpressions));
                            break;

                        case "a":
                            mechanisms.AddLast(new A(qualifier));
                            break;

                        case "mx":
                            mechanisms.AddLast(new Mx(spfStatement, qualifier));
                            break;

                        case "ptr":
                            mechanisms.AddLast(new Ptr(spfStatement, qualifier));
                            break;

                        case "ip4":
                            mechanisms.AddLast(new Ip4(spfStatement, qualifier));
                            break;

                        case "ip6":
                            mechanisms.AddLast(new Ip6(spfStatement, qualifier));
                            break;

                        case "exists":
                            mechanisms.AddLast(new Exists(spfStatement, qualifier));
                            break;

                        case "redirect":
                            if (!(redirect is null))
                                return new KeyValuePair<SpfResult, string>(SpfResult.PermanentError,
                                    "Multiple redirect modifiers found in SPF record.");

                            redirect = new Redirect(spfStatement, qualifier);
                            break;

                        case "exp":
                            if (!(explanation is null))
                                return new KeyValuePair<SpfResult, string>(SpfResult.PermanentError,
                                    "Multiple exp modifiers found in SPF record.");

                            explanation = new Explanation(spfStatement, qualifier);
                            break;

                        default:
                            throw new Exception("Syntax error.");
                    }
                }

                foreach (Mechanism mechanism in mechanisms)
                {
                    await mechanism.Expand();

                    SpfResult result = await mechanism.Matches(dnsHost);

                    switch (result)
                    {
                        case SpfResult.Pass:
                            switch (mechanism.Qualifier)
                            {
                                case SpfQualifier.Pass:
                                    return new KeyValuePair<SpfResult, string>(SpfResult.Pass, null);
                                case SpfQualifier.Fail:
                                    return new KeyValuePair<SpfResult, string>(SpfResult.Fail,
                                        explanation == null ? null : await explanation.Evaluate());
                                case SpfQualifier.Neutral:
                                    return new KeyValuePair<SpfResult, string>(SpfResult.Neutral, null);
                                case SpfQualifier.SoftFail:
                                    return new KeyValuePair<SpfResult, string>(SpfResult.SoftFail,
                                        explanation == null ? null : await explanation.Evaluate());
                            }

                            break;

                        case SpfResult.TemporaryError:
                            return new KeyValuePair<SpfResult, string>(SpfResult.TemporaryError,
                                explanation == null ? null : await explanation.Evaluate());

                        case SpfResult.None:
                        case SpfResult.PermanentError:
                            return new KeyValuePair<SpfResult, string>(SpfResult.PermanentError,
                                explanation == null ? null : await explanation.Evaluate());
                    }
                }

                if (!(redirect is null))
                {
                    await redirect.Expand();

                    string bak = spfStatement.Domain;
                    spfStatement.Domain = redirect.Domain;
                    try
                    {
                        KeyValuePair<SpfResult, string> result =
                            await SpfResolver.CheckHost(spfStatement, spfExpressions, dnsHost);

                        if (result.Key == SpfResult.None)
                            return new KeyValuePair<SpfResult, string>(SpfResult.PermanentError,
                                explanation == null ? null : await explanation.Evaluate());
                        else if (result.Key != SpfResult.Pass && result.Key != SpfResult.Neutral &&
                                 string.IsNullOrEmpty(result.Value))
                        {
                            return new KeyValuePair<SpfResult, string>(result.Key,
                                explanation == null ? null : await explanation.Evaluate());
                        }
                        else
                            return result;
                    }
                    finally
                    {
                        spfStatement.Domain = bak;
                    }
                }
            }
            catch (Exception ex)
            {
                return new KeyValuePair<SpfResult, string>(SpfResult.PermanentError,
                    "Unable to evaluate SPF record: " + FirstRow(ex.Message));
            }

            return new KeyValuePair<SpfResult, string>(SpfResult.Neutral, null);
        }

        private static string FirstRow(string s)
        {
            int i = s.IndexOfAny(Crlf);
            if (i < 0)
                return s;
            else
                return s.Substring(0, i);
        }

        private static readonly char[] Space = new char[] {' '};
        private static readonly char[] Crlf = new char[] {'\r', '\n'};
    }
}