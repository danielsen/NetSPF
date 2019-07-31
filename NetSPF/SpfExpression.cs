using System;

namespace NetSPF
{
    public class SpfExpression
    {
        private readonly string _domainSuffix;
        public string Domain { get; }
        public string Spf { get; }
        public bool IncludeSubDomains { get; }

        public SpfExpression(string domain, string spf, bool includeSubDomains)
        {
            Domain = domain;
            Spf = spf;
            IncludeSubDomains = includeSubDomains;
            _domainSuffix = $"+{domain}";
        }

        public bool IsApplicable(string domain)
        {
            if (String.Compare(Domain, domain, StringComparison.OrdinalIgnoreCase) == 0)
                return true;

            if (IncludeSubDomains && domain.EndsWith(_domainSuffix, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }
    }
}