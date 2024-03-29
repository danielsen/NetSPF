using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace NetSPF.Mechanisms
{
    public class Include : DomainSpecification
    {
        private readonly SpfExpression[] _spfExpressions;

        public Include(SpfStatement spfStatement, SpfQualifier qualifier, params SpfExpression[] spfExpressions)
            : base(spfStatement, qualifier)
        {
            _spfExpressions = spfExpressions;
        }

        public override async Task<SpfResult> Matches(IPAddress dnsHost = null)
        {
            string originalDomain = SpfStatement.Domain;
            SpfStatement.Domain = Domain;
            
            if (SpfStatement.RemainingQueries-- <= 0)
                throw new Exception("DNS Lookup maximum reached.");
            
            try
            {
                KeyValuePair<SpfResult, string> result = await SpfResolver.CheckHostAsync(SpfStatement, _spfExpressions, dnsHost);

                switch (result.Key)
                {
                    case SpfResult.Pass:
                        return SpfResult.Pass;

                    case SpfResult.Fail:
                    case SpfResult.SoftFail:
                    case SpfResult.Neutral:
                        return SpfResult.Fail;

                    case SpfResult.TemporaryError:
                        return SpfResult.TemporaryError;

                    case SpfResult.PermanentError:
                    case SpfResult.None:
                    default:
                        return SpfResult.PermanentError;
                }
            }
            finally
            {
                SpfStatement.Domain = originalDomain;
            }
        }
    }
}