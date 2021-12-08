using System.Net;
using System.Threading.Tasks;

namespace NetSPF.Mechanisms
{
    public abstract class Mechanism
    {
        public SpfQualifier Qualifier { get; }
        public SpfStatement SpfStatement { get; }

        public Mechanism(SpfQualifier qualifier)
        {
            Qualifier = qualifier;
        }

        public virtual Task Expand()
        {
            return Task.CompletedTask;
        }

        public abstract Task<SpfResult> Matches(IPAddress dnsHost = null);
    }
}