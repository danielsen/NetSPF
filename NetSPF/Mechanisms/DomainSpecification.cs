using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetSPF.Mechanisms
{
    public abstract class DomainSpecification : Mechanism
    {
        public string Domain { get; private set; }
        private bool _expanded = false;

        public DomainSpecification(SpfQualifier qualifier) : base(qualifier)
        {
            if (SpfStatement.PeekNextCharacter() == Separator)
            {
                SpfStatement.NextCharacter();

                var start = SpfStatement.Position;
                char ch;

                while ((ch = SpfStatement.PeekNextCharacter()) > ' ' && ch != '/')
                    SpfStatement.Position++;

                Domain = SpfStatement.Statement.Substring(start, SpfStatement.Position - start);
            }
            else if (DomainRequired)
                throw new Exception($"{Separator} expected");
        }

        public override async Task Expand()
        {
            if (_expanded)
                return;

            _expanded = true;
            SpfStatement.Reset(Domain);

            StringBuilder sb = new StringBuilder();
            char ch;

            while ((ch = SpfStatement.PeekNextCharacter()) > ' ')
            {
                SpfStatement.Position++;

                if (ch == '%')
                {
                    switch (ch = SpfStatement.PeekNextCharacter())
                    {
                        case (char) 0:
                            sb.Append('%');
                            break;

                        case '%':
                            SpfStatement.Position++;
                            sb.Append('%');
                            break;

                        case '_':
                            SpfStatement.Position++;
                            sb.Append(' ');
                            break;

                        case '-':
                            SpfStatement.Position++;
                            sb.Append("%20");
                            break;

                        case '{':
                            SpfStatement.Position++;

                            char macroLetter = char.ToLower(SpfStatement.NextCharacter());
                            int? digit;
                            bool reverse;

                            if (char.IsDigit(SpfStatement.PeekNextCharacter()))
                            {
                                digit = SpfStatement.NextInteger();

                                if (digit == 0)
                                    throw new Exception("Invalid number of digits.");
                            }
                            else
                                digit = null;

                            if (char.ToLower(SpfStatement.PeekNextCharacter()) == 'r')
                            {
                                SpfStatement.Position++;
                                reverse = true;
                            }
                            else
                                reverse = false;

                            int start = SpfStatement.Position;
                            while ((ch = SpfStatement.PeekNextCharacter()) == '.' || ch == '-' || ch == '+' ||
                                   ch == ',' || ch == '/' || ch == '_' || ch == '=')
                            {
                                SpfStatement.Position++;
                            }

                            string delimiter =
                                SpfStatement.Statement.Substring(start, SpfStatement.Position - start);

                            ch = SpfStatement.NextCharacter();
                            if (ch != '}')
                                throw new Exception("Expected }");

                            string s;

                            switch (macroLetter)
                            {
                                case 's': // sender
                                    s = SpfStatement.Sender;
                                    break;

                                case 'l': // local-part of sender
                                    s = SpfStatement.Sender;
                                    int i = s.IndexOf('@');
                                    if (i < 0)
                                        s = string.Empty;
                                    else
                                        s = s.Substring(0, i);
                                    break;

                                case 'o': // domain of sender
                                    s = SpfStatement.Sender;
                                    i = s.IndexOf('@');
                                    if (i >= 0)
                                        s = s.Substring(i + 1);
                                    break;

                                case 'd': // domain
                                    s = SpfStatement.Domain;
                                    break;

                                case 'i':
                                    switch (SpfStatement.IpAddress.AddressFamily)
                                    {
                                        case AddressFamily.InterNetwork:
                                            s = SpfStatement.IpAddress.ToString();
                                            break;

                                        case AddressFamily.InterNetworkV6:
                                            byte[] bin = SpfStatement.IpAddress.GetAddressBytes();

                                            StringBuilder sb2 = new StringBuilder();
                                            byte b, b2;

                                            for (i = 0; i < 16; i++)
                                            {
                                                b = bin[i];

                                                b2 = (byte) (b >> 4);
                                                if (b2 < 10)
                                                    sb2.Append((char) ('0' + b2));
                                                else
                                                    sb2.Append((char) ('a' + b2 - 10));

                                                sb2.Append('.');

                                                b2 = (byte) (b & 15);
                                                if (b2 < 10)
                                                    sb2.Append((char) ('0' + b2));
                                                else
                                                    sb2.Append((char) ('a' + b2 - 10));

                                                if (i < 15)
                                                    sb2.Append('.');
                                            }

                                            s = sb2.ToString();
                                            break;

                                        default:
                                            throw new Exception("Invalid client address.");
                                    }

                                    break;

                                case 'p':
                                    try
                                    {
                                        if (SpfStatement.RemainingQueries-- <= 0)
                                            throw new Exception("DNS Lookup maximum reached.");

                                        string[] domainNames =
                                            await DnsResolver.LookupDomainName(SpfStatement.IpAddress);

                                        // First check if domain is found.

                                        s = null;
                                        foreach (string domainName in domainNames)
                                        {
                                            if (String.Compare(domainName, SpfStatement.Domain,
                                                    StringComparison.OrdinalIgnoreCase) == 0 &&
                                                await this.MatchReverseIp(domainName))
                                            {
                                                s = domainName;
                                                break;
                                            }
                                        }

                                        if (s is null)
                                        {
                                            // Second, check if sub-domain is found.

                                            foreach (string domainName in domainNames)
                                            {
                                                if (domainName.EndsWith("." + SpfStatement.Domain,
                                                        StringComparison.CurrentCultureIgnoreCase) &&
                                                    await this.MatchReverseIp(domainName))
                                                {
                                                    s = domainName;
                                                    break;
                                                }
                                            }

                                            if (s is null)
                                            {
                                                if (domainNames.Length == 0)
                                                    s = "unknown";
                                                else
                                                    s = domainNames[DnsResolver.Next(domainNames.Length)];
                                            }
                                        }
                                    }
                                    catch (ArgumentException)
                                    {
                                        s = "unknown";
                                    }
                                    catch (TimeoutException)
                                    {
                                        s = "unknown";
                                    }

                                    break;

                                case 'v':
                                    switch (SpfStatement.IpAddress.AddressFamily)
                                    {
                                        case AddressFamily.InterNetwork:
                                            s = "in-addr";
                                            break;

                                        case AddressFamily.InterNetworkV6:
                                            s = "ip6";
                                            break;

                                        default:
                                            throw new Exception("Invalid client address.");
                                    }

                                    break;

                                case 'h':
                                    s = SpfStatement.HeloDomain;
                                    break;

                                case 'c':
                                    this.AssertExplanation();
                                    s = SpfStatement.IpAddress.ToString();
                                    break;

                                case 'r':
                                    this.AssertExplanation();
                                    s = SpfStatement.HostDomain;
                                    break;

                                case 't':
                                    this.AssertExplanation();
                                    int seconds = (int) Math.Round((DateTime.UtcNow - UnixEpoch).TotalSeconds);
                                    s = seconds.ToString();
                                    break;

                                default:
                                    throw new Exception("Unknown macro.");
                            }

                            if (reverse || digit.HasValue || !string.IsNullOrEmpty(delimiter))
                            {
                                if (string.IsNullOrEmpty(delimiter))
                                    delimiter = ".";

                                string[] parts = s.Split(new string[] {delimiter}, StringSplitOptions.None);
                                int i = parts.Length;

                                if (reverse)
                                    Array.Reverse(parts);

                                if (digit.HasValue && digit.Value < i)
                                    i = digit.Value;

                                bool first = true;
                                int j = parts.Length - i;

                                while (i-- > 0)
                                {
                                    if (first)
                                        first = false;
                                    else
                                        sb.Append('.');

                                    sb.Append(parts[j++]);
                                }
                            }
                            else
                                sb.Append(s);

                            break;

                        default:
                            SpfStatement.Position++;
                            sb.Append('%');
                            sb.Append(ch);
                            break;
                    }
                }
                else
                    sb.Append(ch);
            }

            Domain = sb.ToString();
        }

        internal async Task<bool> MatchReverseIp(string domainName)
        {
            if (SpfStatement.RemainingQueries-- <= 0)
                throw new Exception("DNS Lookup maximum reached.");

            IPAddress[] addresses;

            switch (SpfStatement.IpAddress.AddressFamily)
            {
                case AddressFamily.InterNetwork:
                    addresses = await DnsResolver.LookupIp4Addresses(domainName);
                    break;

                case AddressFamily.InterNetworkV6:
                    addresses = await DnsResolver.LookupIp6Addresses(domainName);
                    break;

                default:
                    throw new Exception("Invalid client address.");
            }

            var ipString = SpfStatement.IpAddress.ToString();

            return addresses.Any(addr =>
                String.Compare(addr.ToString(), ipString, StringComparison.OrdinalIgnoreCase) == 0);
        }

        private void AssertExplanation()
        {
            if (!(this is Explanation))
                throw new Exception("Macro only available in expression.");
        }

        public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public virtual char Separator => ':';

        public virtual bool DomainRequired => true;

        public string TargetDomain => string.IsNullOrEmpty(Domain) ? SpfStatement.Domain : Domain;
    }
}