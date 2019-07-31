using System;
using System.Net;

namespace NetSPF
{
    public class SpfStatement
    {
        internal string Statement;
        internal int Position = 0;
        internal int RemainingQueries = 10;
        internal readonly string Sender;
        internal string Domain;
        internal readonly string HeloDomain;
        internal readonly string HostDomain;
        internal readonly IPAddress IpAddress;

        public SpfStatement(string sender, string domain, IPAddress ipAddress,
            string heloDomain, string hostDomain)
        {
            Sender = sender;
            Domain = domain;
            IpAddress = ipAddress;
            HeloDomain = heloDomain;
            HostDomain = hostDomain;
        }

        public void Reset(string statement)
        {
            Statement = statement?.Trim() ?? string.Empty;
            Position = 0;
        }

        internal void SkipWhitespace()
        {
            while (Position < Statement.Length && Statement[Position] <= ' ')
                Position++;
        }

        internal char PeekNextCharacter()
        {
            if (Position >= Statement.Length)
                return (char) 0;

            return Statement[Position];
        }

        internal char NextCharacter()
        {
            if (Position >= Statement.Length)
                throw new Exception("SPF syntax error.");

            return Statement[Position++];
        }

        internal int NextInteger()
        {
            var start = Position;

            while (char.IsDigit(PeekNextCharacter()))
                Position++;

            return int.Parse(Statement.Substring(start, Position - start));
        }

        internal string NextLabel()
        {
            var start = Position;
            char ch;

            if (!char.IsLetter(ch = PeekNextCharacter())) 
                return Statement.Substring(start, Position - start);
            
            Position++;

            while (char.IsLetter(ch = PeekNextCharacter()) || char.IsDigit(ch) || ch == '-' || ch == '_' ||
                   ch == '.')
            {
                Position++;
            }

            return Statement.Substring(start, Position - start);
        }
    }
}