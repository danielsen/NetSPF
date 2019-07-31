using System.Threading.Tasks;

namespace NetSPF.Mechanisms
{
    public abstract class Mechanism
    {
        public SpfQualifier Qualifier { get; }
        public SpfStatement SpfStatement { get; }

        public Mechanism(SpfStatement spfStatement, SpfQualifier qualifier)
        {
            Qualifier = qualifier;
            SpfStatement = spfStatement;
        }

        public virtual Task Expand()
        {
            return Task.CompletedTask;
        }

        public abstract Task<SpfResult> Matches();
    }
}