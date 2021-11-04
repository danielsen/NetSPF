using System.Threading.Tasks;

namespace NetSPF.Mechanisms
{
    public class All : Mechanism
    {
        public All(SpfStatement spfStatement, SpfQualifier qualifier) : base(spfStatement, qualifier)
        {
        }

        public override Task<SpfResult> Matches()
        {
            return Task.FromResult<SpfResult>(SpfResult.Pass);
        }
    }
}