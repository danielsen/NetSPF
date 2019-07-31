using System;
using System.Net.Sockets;

namespace NetSPF.Mechanisms
{
    public class Ip6 : Ip
    {
        public Ip6(SpfStatement spfStatement, SpfQualifier qualifier) : base(spfStatement, qualifier)
        {
            if (IpAddress.AddressFamily != AddressFamily.InterNetworkV6)
                throw new Exception("IPv6 address expected.");
        }
    }
}