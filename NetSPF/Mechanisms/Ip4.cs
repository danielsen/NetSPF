using System;
using System.Net.Sockets;

namespace NetSPF.Mechanisms
{
    public class Ip4 : Ip
    {
        public Ip4(SpfStatement spfStatement, SpfQualifier qualifier) : base(spfStatement, qualifier)
        {
            if (IpAddress.AddressFamily != AddressFamily.InterNetwork)
                throw new Exception("IPv4 address expected.");
        }
    }
}