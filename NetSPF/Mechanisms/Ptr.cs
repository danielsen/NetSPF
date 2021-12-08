using System;
using System.Net;
using System.Threading.Tasks;

namespace NetSPF.Mechanisms
{
    public class Ptr : DomainSpecification
    {
        public Ptr(SpfStatement spfStatement, SpfQualifier qualifier) : base(qualifier)
        {
        }

        public override bool DomainRequired => false;

        public override async Task<SpfResult> Matches(IPAddress dnsHost = null)
        {
            try
            {
                if (SpfStatement.RemainingQueries-- <= 0)
                    throw new Exception("DNS Lookup maximum reached.");

                string targetDomain = TargetDomain;
                string[] domainNames = await DnsResolver.LookupDomainName(SpfStatement.IpAddress, dnsHost);

                // First check if domain is found.

                foreach (string domainName in domainNames)
                {
                    if (String.Compare(domainName, targetDomain, StringComparison.OrdinalIgnoreCase) == 0 &&
                        await MatchReverseIp(domainName))
                    {
                        return SpfResult.Pass;
                    }
                }

                // Second, check if sub-domain is found.

                foreach (string domainName in domainNames)
                {
                    if (domainName.EndsWith("." + targetDomain, StringComparison.OrdinalIgnoreCase) &&
                        await MatchReverseIp(domainName))
                    {
                        return SpfResult.Pass;
                    }
                }
            }
            catch (Exception)
            {
                // Fail
            }

            return SpfResult.Fail;
        }
    }
}