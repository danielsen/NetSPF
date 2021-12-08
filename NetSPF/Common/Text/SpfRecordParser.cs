using System;
using System.Globalization;
using System.Linq;
using NetSPF.Mechanisms;

namespace NetSPF.Common.Text
{
    public class SpfRecordParser : TokenParser
    {
        public SpfRecordParser(ITokenEnumerator enumerator) : base(enumerator)
        {
        }

        internal static class Tokens
        {
            internal static readonly Token Colon = Token.Create(TokenType.Separator, ':');
            internal static readonly Token Equal = Token.Create(TokenType.Separator, '=');
            internal static readonly Token Hyphen = Token.Create('-');
            internal static readonly Token Period = Token.Create('.');
            internal static readonly Token Plus = Token.Create(TokenType.Qualifier, '+');
            internal static readonly Token Question = Token.Create(TokenType.Qualifier, '?');
            internal static readonly Token Slash = Token.Create(TokenType.Separator, ',');
            internal static readonly Token Tilde = Token.Create(TokenType.Qualifier, '~');

            internal static class Text
            {
                internal static readonly Token A = Token.Create("a");
                internal static readonly Token All = Token.Create("all");
                internal static readonly Token Exists = Token.Create("exists");
                internal static readonly Token Exp = Token.Create("exp");
                internal static readonly Token Include = Token.Create("include");
                internal static readonly Token Ip4 = Token.Create("ip4");
                internal static readonly Token Ip6 = Token.Create("ip6");
                internal static readonly Token Mx = Token.Create("mx");
                internal static readonly Token Ptr = Token.Create("ptr");
                internal static readonly Token Redirect = Token.Create("redirect");
                internal static readonly Token Version = Token.Create("v=spf1");
            }
        }
        
        #region private

        private void ReadToNextBlock(out string read)
        {
            read = string.Empty;

            while (Enumerator.Peek().Type != TokenType.Space && Enumerator.Peek() != Token.None)
            {
                read += Enumerator.Take().Text;
            }
        }
        
        #endregion

        #region subroutines

        /// <summary>
        /// Try to make 16 bits hex number.
        /// </summary>
        /// <param name="hexNumber">Extracted hex number.</param>
        /// <returns>true if valid hex number can be extracted.</returns>
        public bool TryMake16BitHexNumber(out string hexNumber)
        {
            hexNumber = null;

            var token = Enumerator.Peek();
            while (token.Type == TokenType.Number || token.Type == TokenType.Text)
            {
                if (hexNumber != null && (hexNumber.Length + token.Text.Length) > 4)
                {
                    return false;
                }

                if (token.Type == TokenType.Text && IsHex(token.Text) == false)
                {
                    return false;
                }

                hexNumber = string.Concat(hexNumber ?? string.Empty, token.Text);

                Enumerator.Take();
                token = Enumerator.Peek();
            }

            return true;

            bool IsHex(string text)
            {
                return text.ToUpperInvariant().All(c => c >= 'A' && c <= 'F');
            }
        }

        /// <summary>
        /// Try to make a address.
        /// </summary>
        /// <param name="address">The address that was made, or undefined if it was not made.</param>
        /// <returns>true if the address was made, false if not.</returns>
        /// <remarks><![CDATA[( IPv4-address-literal / IPv6-address-literal / General-address-literal )]]></remarks>
        public bool TryMakeAddressLiteral(out string address)
        {
            address = null;

            // skip any whitespace
            Enumerator.Skip(TokenType.Space);

            if (TryMake(TryMakeIpv4AddressLiteral, out address) == false &&
                TryMake(TryMakeIpv6AddressLiteral, out address) == false)
            {
                return false;
            }

            // skip any whitespace
            Enumerator.Skip(TokenType.Space);

            return address != null;
        }

        /// <summary>
        /// Try to make a domain name.
        /// </summary>
        /// <param name="domain">The domain name that was made, or undefined if it was not made.</param>
        /// <returns>true if the domain name was made, false if not.</returns>
        /// <remarks><![CDATA[sub-domain *("." sub-domain)]]></remarks>
        public bool TryMakeDomain(out string domain)
        {
            if (TryMake(TryMakeSubdomain, out domain) == false)
            {
                return false;
            }

            while (Enumerator.Peek() == Tokens.Period)
            {
                Enumerator.Take();

                if (TryMake(TryMakeSubdomain, out string subdomain) == false)
                {
                    return false;
                }

                domain += string.Concat(".", subdomain);
            }

            return true;
        }

        /// <summary>
        /// Try to make an IPv4 address literal.
        /// </summary>
        /// <param name="address">The address that was made, or undefined if it was not made.</param>
        /// <returns>true if the address was made, false if not.</returns>
        /// <remarks><![CDATA[ Snum 3("."  Snum) ]]></remarks>
        public bool TryMakeIpv4AddressLiteral(out string address)
        {
            address = null;

            if (TryMake(TryMakeShortNum, out int snum) == false)
            {
                return false;
            }

            address = snum.ToString(CultureInfo.InvariantCulture);

            for (var i = 0; i < 3 && Enumerator.Peek() == Tokens.Period; i++)
            {
                Enumerator.Take();

                if (TryMake(TryMakeShortNum, out snum) == false)
                {
                    return false;
                }

                address = string.Concat(address, '.', snum);
            }

            return true;
        }

        /// <summary>
        /// Try to extract IPv6 address. https://tools.ietf.org/html/rfc4291 section 2.2 used for specification.
        /// </summary>
        /// <param name="address">Extracted Ipv6 address.</param>
        /// <returns>true if a valid Ipv6 address can be extracted.</returns>
        public bool TryMakeIpv6AddressLiteral(out string address)
        {
            address = null;

            var hasDoubleColumn = false;
            var hexPartCount = 0;
            var hasIpv4Part = false;
            var wasColon = false;

            var token = Enumerator.Peek();
            var builder = new System.Text.StringBuilder();
            while (token.Type == TokenType.Number || token.Type == TokenType.Text || token == Tokens.Colon)
            {
                using (var cp = Enumerator.Checkpoint())
                {
                    Enumerator.Take();
                    // Alternate form with mixed IPv6 and IPv4 formats. See https://tools.ietf.org/html/rfc4291 section 2.2 item 3
                    if (token.Type == TokenType.Number && Enumerator.Peek() == Tokens.Period)
                    {
                        cp.Rollback();
                        if (TryMake(TryMakeIpv4AddressLiteral, out string ipv4))
                        {
                            hasIpv4Part = true;
                            builder.Append(ipv4);
                            break;
                        }

                        return false;
                    }

                    cp.Rollback();
                }

                if (token == Tokens.Colon)
                {
                    if (wasColon)
                    {
                        // Double column is allowed only once
                        if (hasDoubleColumn)
                        {
                            return false;
                        }

                        hasDoubleColumn = true;
                    }

                    builder.Append(token.Text);
                    wasColon = true;
                    Enumerator.Take();
                }
                else
                {
                    if (wasColon == false && builder.Length > 0)
                    {
                        return false;
                    }

                    wasColon = false;
                    if (TryMake(TryMake16BitHexNumber, out string hexNumber))
                    {
                        builder.Append(hexNumber);
                        hexPartCount++;
                    }
                    else
                    {
                        return false;
                    }
                }

                token = Enumerator.Peek();
            }

            address = builder.ToString();

            var maxAllowedParts = (hasIpv4Part ? 6 : 8) - Math.Sign(hasDoubleColumn ? 1 : 0);
            if ((hasDoubleColumn && hexPartCount > maxAllowedParts) ||
                (!hasDoubleColumn && hexPartCount != maxAllowedParts))
            {
                return false;
            }

            return true;
        }

        public bool TryMakeQualifier(out SpfQualifier qualifier)
        {
            qualifier = SpfQualifier.Neutral;

            var token = Enumerator.Take();
            if (token.Type != TokenType.Qualifier)
            {
                return false;
            }

            return EnumParser.TryParse(token.Text, out qualifier);
        }

        public bool TryMakeSeparator(out string separator)
        {
            var token = Enumerator.Take();
            separator = token.Text;

            return token.Type == TokenType.Separator;
        }

        /// <summary>
        /// Try to make an Snum (number in the range of 0-255).
        /// </summary>
        /// <param name="snum">The snum that was made, or undefined if it was not made.</param>
        /// <returns>true if the snum was made, false if not.</returns>
        /// <remarks><![CDATA[ 1*3DIGIT ]]></remarks>
        public bool TryMakeShortNum(out int snum)
        {
            snum = default(int);

            var token = Enumerator.Take();

            if (token.Type == TokenType.Number && int.TryParse(token.Text, out snum))
            {
                return snum >= 0 && snum <= 255;
            }

            return false;
        }

        /// <summary>
        /// Try to make a subdomain name.
        /// </summary>
        /// <param name="subdomain">The subdomain name that was made, or undefined if it was not made.</param>
        /// <returns>true if the subdomain name was made, false if not.</returns>
        /// <remarks><![CDATA[Let-dig [Ldh-str]]]></remarks>
        public bool TryMakeSubdomain(out string subdomain)
        {
            if (TryMake(TryMakeTextOrNumber, out subdomain) == false)
            {
                return false;
            }

            if (TryMake(TryMakeTextOrNumberOrHyphenString, out string letterNumberHyphen) == false)
            {
                return subdomain != null;
            }

            subdomain += letterNumberHyphen;

            return true;
        }

        /// <summary>
        /// Try to make text.
        /// </summary>
        /// <param name="text">The text that was mode or undefined if it was not made.</param>
        /// <returns><![CDATA[ALPHA]]></returns>
        public bool TryMakeText(out string text)
        {
            var token = Enumerator.Take();
            text = token.Text;

            return token.Type == TokenType.Text;
        }

        /// <summary>
        /// Try to make a text or number
        /// </summary>
        /// <param name="textOrNumber">The text or number that was made, or undefined if it was not made.</param>
        /// <returns>true if the text or number was made, false if not.</returns>
        /// <remarks><![CDATA[ALPHA / DIGIT]]></remarks>
        public bool TryMakeTextOrNumber(out string textOrNumber)
        {
            var token = Enumerator.Take();

            textOrNumber = token.Text;

            return token.Type == TokenType.Text || token.Type == TokenType.Number;
        }

        /// <summary>
        /// Try to make a text/number/hyphen string.
        /// </summary>
        /// <param name="textOrNumberOrHyphenString">The text, number, or hyphen that was matched, or undefined if it was not matched.</param>
        /// <returns>true if a text, number or hyphen was made, false if not.</returns>
        /// <remarks><![CDATA[*( ALPHA / DIGIT / "-" ) Let-dig]]></remarks>
        public bool TryMakeTextOrNumberOrHyphenString(out string textOrNumberOrHyphenString)
        {
            textOrNumberOrHyphenString = null;

            var token = Enumerator.Peek();
            while (token.Type == TokenType.Text || token.Type == TokenType.Number || token == Tokens.Hyphen)
            {
                textOrNumberOrHyphenString += Enumerator.Take().Text;

                token = Enumerator.Peek();
            }

            // can not end with a hyphen
            return textOrNumberOrHyphenString != null && token != Tokens.Hyphen;
        }

        #endregion

        #region mechanismParsers

        public bool TryMakeA(out A mechanism, out ParsingError error)
        {
            error = null;
            mechanism = null;
            Enumerator.Skip(TokenType.Space);

            TryMake(TryMakeQualifier, out SpfQualifier qualifier);

            if (TryMake(TryMakeText, out string text) == false)
            {
                return false;
            }

            if (text != Tokens.Text.A.Text)
            {
                return false;
            }

            return true;

        }
        
        public bool TryMakeAll(out All mechanism, out ParsingError error)
        {
            error = null;
            mechanism = null;
            Enumerator.Skip(TokenType.Space);

            TryMake(TryMakeQualifier, out SpfQualifier qualifier);

            if (TryMake(TryMakeText, out string text) == false)
            {
                ReadToNextBlock(out text);
                error = ParsingError.CreateForInvalidQualifier(text);
                return false;
            }

            if (text != Tokens.Text.All.Text)
            {
                return false;
            }

            mechanism = new All(qualifier);
            return true;
        }
        
        #endregion
    }
}