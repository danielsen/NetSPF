using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Resolution.Common;
using Resolution.Protocol;
using Resolution.Protocol.Records;

namespace NetSPF
{
    public static class DnsResolver
    {
        private static readonly Resolver Resolver;
        private static readonly Random Random = new Random();

        static DnsResolver()
        {
            Resolver = ResolverFactory.GetResolver(transportType: TransportType.Tcp);
        }

        public static async Task<IPAddress[]> LookupIp4Addresses(string domainLabel, IPAddress dnsHost = null)
        {
            var resolver = dnsHost != null
                ? ResolverFactory.GetResolver(dnsServers: new[] { dnsHost.ToString() }, transportType: TransportType.Tcp)
                : Resolver;
            var response = await Task.Run(() => resolver.Query(domainLabel, QuestionType.A, QuestionClass.In));
            return response.GetAnswers<RecordA>().Select(r => r.Address).ToArray();
        }

        public static async Task<IPAddress[]> LookupIp6Addresses(string domainLabel, IPAddress dnsHost = null)
        {
            var resolver = dnsHost != null
                ? ResolverFactory.GetResolver(dnsServers: new[] { dnsHost.ToString() }, transportType: TransportType.Tcp)
                : Resolver;
            var response = await Task.Run(() => resolver.Query(domainLabel, QuestionType.Aaaa, QuestionClass.In));
            return response.GetAnswers<RecordAaaa>().Select(r => r.Address).ToArray();
        }

        public static async Task<string[]> LookupDomainName(IPAddress ip, IPAddress dnsHost = null)
        {
            var resolver = dnsHost != null
                ? ResolverFactory.GetResolver(dnsServers: new[] { dnsHost.ToString() }, transportType: TransportType.Tcp)
                : Resolver;
            var response = await Task.Run(() => resolver.Query(ip.ToString(), QuestionType.Ptr, QuestionClass.In));
            return response.GetAnswers<RecordPtr>().Select(r => r.Ptrdname).ToArray();
        }

        public static async Task<string[]> LookupMailExchange(string domainLabel, IPAddress dnsHost = null)
        {
            var resolver = dnsHost != null
                ? ResolverFactory.GetResolver(dnsServers: new[] { dnsHost.ToString() }, transportType: TransportType.Tcp)
                : Resolver;
            var response = await Task.Run(() => resolver.Query(domainLabel, QuestionType.Mx, QuestionClass.In));
            return response.GetAnswers<RecordMx>().Select(r => r.Exchange).ToArray();
        }

        public static async Task<string[]> LookupText(string domainLabel, IPAddress dnsHost = null)
        {
            var resolver = dnsHost != null
                ? ResolverFactory.GetResolver(dnsServers: new[] { dnsHost.ToString() }, transportType: TransportType.Tcp)
                : Resolver;
            var response = await Task.Run(() => resolver.Query(domainLabel, QuestionType.Txt, QuestionClass.In));
            return response.GetAnswers<RecordTxt>().Select(r => r.ToString()).ToArray();
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