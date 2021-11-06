using System.Net;
using System.Threading.Tasks;

namespace NetSPF.Mechanisms
{
    public class All : Mechanism
    {
        public All(SpfStatement spfStatement, SpfQualifier qualifier) : base(spfStatement, qualifier)
        {
        }

        public override Task<SpfResult> Matches(IPAddress dnsHost = null)
        {
            return Task.FromResult<SpfResult>(SpfResult.Pass);
        }
    }
}