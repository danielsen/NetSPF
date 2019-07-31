using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetSPF.Mechanisms
{
    public class A : DomainCidrSpecification
    {
        public A(SpfStatement statement, SpfQualifier spfQualifier) : base(statement, spfQualifier)
        {
        }

        public override async Task<SpfResult> Matches()
        {
            return await Matches(TargetDomain, SpfStatement, Ipv4Cidr, Ipv6Cidr) ? SpfResult.Pass : SpfResult.Fail;
        }

        internal static async Task<bool> Matches(string domain, SpfStatement statement, int cidr4, int cidr6)
        {
            IPAddress[] addresses;

            int cidr;
            switch (statement.IpAddress.AddressFamily)
            {
                case AddressFamily.InterNetwork:
                    if (statement.RemainingQueries-- <= 0)
                        throw new Exception("DNS Lookup maximum reached.");

                    addresses = await DnsResolver.LookupIp4Addresses(domain);
                    cidr = cidr4;
                    break;

                case AddressFamily.InterNetworkV6:
                    if (statement.RemainingQueries-- <= 0)
                        throw new Exception("DNS Lookup maximum reached.");

                    addresses = await DnsResolver.LookupIp6Addresses(domain);
                    cidr = cidr6;
                    break;

                default:
                    return false;
            }

            return Matches(addresses, statement, cidr);
        }
    }
}