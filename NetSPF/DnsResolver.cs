using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Resolution.Protocol;

namespace NetSPF
{
    public static class DnsResolver
    {
        private static readonly Resolver Resolver;
        private static readonly Random Random = new Random();

        static DnsResolver()
        {
            Resolver = new Resolver();
        }

        public static async Task<IPAddress[]> LookupIp4Addresses(string domainLabel)
        {
            var response = await Task.Run(() => Resolver.Query(domainLabel, QType.A, QClass.In));
            return response.RecordsA.Select(r => r.Address).ToArray();
        }

        public static async Task<IPAddress[]> LookupIp6Addresses(string domainLabel)
        {
            var response = await Task.Run(() => Resolver.Query(domainLabel, QType.Aaaa, QClass.In));
            return response.RecordsAaaa.Select(r => r.Address).ToArray();
        }

        public static async Task<string[]> LookupDomainName(IPAddress ip)
        {
            var response = await Task.Run(() => Resolver.Query(ip.ToString(), QType.Ptr, QClass.In));
            return response.RecordsPtr.Select(r => r.Ptrdname).ToArray();
        }

        public static async Task<string[]> LookupMailExchange(string domainLabel)
        {
            var response = await Task.Run(() => Resolver.Query(domainLabel, QType.Mx, QClass.In));
            return response.RecordsMx.Select(r => r.Exchange).ToArray();
        }

        public static async Task<string[]> LookupText(string domainLabel)
        {
            var response = await Task.Run(() => Resolver.Query(domainLabel, QType.Txt, QClass.In));
            return response.RecordsTxt.Select(r => r.ToString()).ToArray();
        }

        public static int Next(int maxValue)
        {
            lock (Random)
            {
                return Random.Next(maxValue);
            }
        }
    }
}