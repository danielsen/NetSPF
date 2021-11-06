using System;
using System.Net;
using System.Threading.Tasks;

namespace NetSPF.Mechanisms
{
    public class Exists : DomainSpecification
    {
        public Exists(SpfStatement spfStatement, SpfQualifier qualifier) : base(spfStatement, qualifier)
        {
        }

        public override async Task<SpfResult> Matches(IPAddress dnsHost = null)
        {
            if (SpfStatement.RemainingQueries-- <= 0)
                throw new Exception("DNS lookup maximum reached.");

            try
            {
                IPAddress[] addresses = await DnsResolver.LookupIp4Addresses(Domain, dnsHost);
                if (addresses is null || addresses.Length == 0)
                    return SpfResult.Fail;
                
                return SpfResult.Pass;
            }
            catch (Exception)
            {
                return SpfResult.Fail;
            }
        }
    }
}