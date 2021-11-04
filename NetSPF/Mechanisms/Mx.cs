using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetSPF.Mechanisms
{
    public class Mx : DomainCidrSpecification
    {
        public Mx(SpfStatement statement, SpfQualifier spfQualifier) : base(statement, spfQualifier)
        {
        }

        public override async Task<SpfResult> Matches()
        {
            if (SpfStatement.RemainingQueries-- <= 0)
                throw new Exception("DNS Lookup maximum reached.");

            string targetDomain = TargetDomain;
            string[] exchanges = await DnsResolver.LookupMailExchange(TargetDomain);

            foreach (string exchange in exchanges)
            {
                IPAddress[] addresses;
                int cidr;

                switch (SpfStatement.IpAddress.AddressFamily)
                {
                    case AddressFamily.InterNetwork:
                        if (SpfStatement.RemainingQueries-- <= 0)
                            throw new Exception("DNS Lookup maximum reached.");

                        addresses = await DnsResolver.LookupIp4Addresses(exchange);
                        cidr = Ipv4Cidr;
                        break;

                    case AddressFamily.InterNetworkV6:
                        if (SpfStatement.RemainingQueries-- <= 0)
                            throw new Exception("DNS Lookup maximum reached.");

                        addresses = await DnsResolver.LookupIp6Addresses(exchange);
                        cidr = Ipv6Cidr;
                        break;

                    default:
                        return SpfResult.Fail;
                }

                if (Matches(addresses, SpfStatement, cidr))
                    return SpfResult.Pass;
            }

            return SpfResult.Fail;
        }
    }
}