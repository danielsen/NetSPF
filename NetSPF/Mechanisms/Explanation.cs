using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetSPF.Mechanisms
{
    public class Explanation : DomainSpecification
    {
        public Explanation(SpfStatement spfStatement, SpfQualifier qualifier) : base(spfStatement, qualifier)
        {
        }

        public override char Separator => '=';

        public override Task<SpfResult> Matches(IPAddress dnsHost = null)
        {
            return Task.FromResult(SpfResult.Fail);
        }

        public async Task<string> Evaluate()
        {
            try
            {
                await Expand();

                StringBuilder sb = new StringBuilder();

                foreach (string text in await DnsResolver.LookupText(Domain))
                    sb.Append(text); // No white-space delimiter

                SpfStatement.Reset("=" + sb.ToString());
                Explanation explanation = new Explanation(SpfStatement, Qualifier);

                await explanation.Expand();

                return explanation.Domain;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}