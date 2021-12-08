using System.Net;
using System.Threading.Tasks;

namespace NetSPF.Mechanisms
{
    public class Redirect : DomainSpecification
    {
        public Redirect(SpfStatement spfStatement, SpfQualifier qualifier) : base(qualifier)
        {
        }

        public override char Separator => '=';
        
        public override Task<SpfResult> Matches(IPAddress dnsHost = null)
        {
            return Task.FromResult<SpfResult>(SpfResult.Fail);
        }
    }
}