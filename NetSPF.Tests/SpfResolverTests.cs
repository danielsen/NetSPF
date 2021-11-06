using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;

namespace NetSPF.Tests
{
    [TestFixture]
    public class SpfResolverTests
    {
        [TestCase("192.30.252.206", "github.com", "noreply@github.com", "out-23.smtp.github.com", "nielsen.com",
            SpfResult.Pass)]
        [TestCase("199.30.252.206", "github.com", "noreply@github.com", "out-23.smtp.github.com", "nielsen.com",
            SpfResult.SoftFail)]
        public async Task should_get_correct_spf_result(string ipAddress, string domain, string sender,
            string heloDomain, string hostDomain, SpfResult expectedResult)
        {
            var dnsHost = IPAddress.Parse("1.1.1.1");
            KeyValuePair<SpfResult, string> result = await SpfResolver.CheckHost(IPAddress.Parse(ipAddress),
                domain, sender, heloDomain, hostDomain, dnsHost);
            
            Assert.AreEqual(expectedResult, result.Key, result.Value);
        }
    }
}