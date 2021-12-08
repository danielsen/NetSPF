using System.Net;
using System.Threading.Tasks;

namespace NetSPF.Mechanisms
{
    public class All : Mechanism
    {
        public All(SpfQualifier qualifier) : base(qualifier)
        {
        }

        public override Task<SpfResult> Matches(IPAddress dnsHost = null)
        {
            return Task.FromResult<SpfResult>(SpfResult.Pass);
        }
    }
}