using System;
using System.Net;

namespace NetSPF.Mechanisms
{
    public abstract class DomainCidrSpecification : DomainSpecification
    {
        protected readonly int Ipv4Cidr = 32;
        protected readonly int Ipv6Cidr = 128;

        public DomainCidrSpecification(SpfQualifier spfQualifier)
            : base(spfQualifier)
        {
            /*
            if (statement.PeekNextCharacter() == '/')
            {
                statement.NextCharacter();
                bool hasIpv4;

                if (hasIpv4 = char.IsDigit(statement.PeekNextCharacter()))
                {
                    Ipv4Cidr = statement.NextInteger();
                    if (Ipv4Cidr < 0 || Ipv4Cidr > 32)
                        throw new Exception("Invalid IPv4 CIDR");
                }

                if (statement.PeekNextCharacter() == '/')
                {
                    statement.NextCharacter();

                    if (hasIpv4 && statement.PeekNextCharacter() == '/')
                    {
                        statement.NextCharacter();
                    }

                    if (char.IsDigit(statement.PeekNextCharacter()))
                    {
                        Ipv6Cidr = statement.NextInteger();
                        if (Ipv6Cidr < 0 || Ipv6Cidr > 128)
                            throw new Exception("Invalid IPv6 CIDR");
                    }
                    else if (!hasIpv4)
                        throw new Exception("IPv4 or IPv6 CIDR expected.");
                }
            }
            */
        }

        public override bool DomainRequired => false;

        internal static bool Matches(IPAddress[] addresses, SpfStatement statement, int cidr)
        {
            byte[] statementAddressBytes = statement.IpAddress.GetAddressBytes();
            int c = statementAddressBytes.Length;

            foreach (IPAddress addr in addresses)
            {
                byte[] addressBytes = addr.GetAddressBytes();
                if (addressBytes.Length != c)
                    continue;

                int bitsLeft = cidr;
                int pos = 0;

                while (bitsLeft > 0 && pos < c)
                {
                    if (bitsLeft >= 8)
                    {
                        if (statementAddressBytes[pos] != addressBytes[pos])
                            break;

                        bitsLeft -= 8;
                    }
                    else
                    {
                        byte mask = (byte) (0xff << (8 - bitsLeft));

                        if ((statementAddressBytes[pos] & mask) != (addressBytes[pos] & mask))
                            break;

                        bitsLeft = 0;
                    }

                    pos++;
                }

                if (bitsLeft == 0)
                    return true;
            }

            return false;
        }
    }
}