using System.Threading.Tasks;

namespace NetSPF.Mechanisms
{
    public class Redirect : DomainSpecification
    {
        public Redirect(SpfStatement spfStatement, SpfQualifier qualifier) : base(spfStatement, qualifier)
        {
        }

        public override char Separator => '=';
        
        public override Task<SpfResult> Matches()
        {
            return Task.FromResult<SpfResult>(SpfResult.Fail);
        }
    }
}