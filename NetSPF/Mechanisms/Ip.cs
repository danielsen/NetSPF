using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetSPF.Mechanisms
{
    public class Ip : Mechanism
    {
        protected readonly IPAddress IpAddress;
        protected readonly int Cidr;

        public Ip(SpfStatement spfStatement, SpfQualifier qualifier) : base(spfStatement, qualifier)
        {
            if (SpfStatement.PeekNextCharacter() != ':')
                throw new Exception(": expected.");

            SpfStatement.NextCharacter();

            int start = SpfStatement.Position;
            char ch;

            while (SpfStatement.Position < SpfStatement.Statement.Length &&
                   (ch = SpfStatement.Statement[SpfStatement.Position]) != '/' && ch > ' ')
                SpfStatement.Position++;

            if (!IPAddress.TryParse(SpfStatement.Statement.Substring(start, SpfStatement.Position - start),
                out IpAddress))
                throw new Exception("IP Address expected.");

            int max;

            switch (IpAddress.AddressFamily)
            {
                case AddressFamily.InterNetwork:
                    max = 32;
                    break;

                case AddressFamily.InterNetworkV6:
                    max = 128;
                    break;

                default:
                    throw new Exception("IP Address expected.");
            }

            if (SpfStatement.PeekNextCharacter() == '/')
            {
                SpfStatement.NextCharacter();

                Cidr = SpfStatement.NextInteger();
                if (Cidr < 0 || Cidr > max)
                    throw new Exception("Invalid CIDR");
            }
            else
                Cidr = max;
        }

        public override Task<SpfResult> Matches()
        {
            bool result = DomainCidrSpecification.Matches(new IPAddress[] {IpAddress}, SpfStatement, Cidr);
            return Task.FromResult<SpfResult>(result ? SpfResult.Pass : SpfResult.Fail);
        }
    }
}