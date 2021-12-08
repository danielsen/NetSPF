using System;
using System.Text;
using NetSPF.Common.Text;
using NetSPF.Mechanisms;
using NUnit.Framework;

namespace NetSPF.Tests.Unit.Common.Text
{
    public class SpfRecordParserTests
    {
        private static SpfRecordParser CreateParser(string text)
        {
            var segment = new ArraySegment<byte>(Encoding.UTF8.GetBytes(text));

            return new SpfRecordParser(new TokenEnumerator(new ByteArrayTokenReader(new[] {segment})));
        }

        [Test]
        public void should_make_text_or_number([Values("abc", "123")] string input)
        {
            var made = CreateParser(input).TryMakeTextOrNumber(out var result);
            Assert.True(made);
            Assert.AreEqual(input, result);
        }

        [Test]
        public void should_make_text_or_number_or_hyphen_string()
        {
            var parser = CreateParser("a1-b2");

            var made1 = parser.TryMakeTextOrNumberOrHyphenString(out var textOrNumberOrHyphen1);

            Assert.True(made1);
            Assert.AreEqual("a1-b2", textOrNumberOrHyphen1);
        }

        [Test]
        public void should_make_sub_domain()
        {
            var parser = CreateParser("a-1-b-2");

            var made = parser.TryMakeSubdomain(out var subdomain);

            Assert.True(made);
            Assert.AreEqual("a-1-b-2", subdomain);
        }

        [Test]
        public void should_make_domain()
        {
            var parser = CreateParser("123.abc.com");

            var made = parser.TryMakeDomain(out var domain);

            Assert.True(made);
            Assert.AreEqual("123.abc.com", domain);
        }

        [Test]
        public void should_make_short_number([Values("1", "236", "301", "abc")] string input)
        {
            var parser = CreateParser(input);
            var made = parser.TryMakeShortNum(out int snum);
            Assert.AreEqual(snum is not (0 or > 255), made);
        }

        [Test]
        public void should_make_ipv4_address_literal()
        {
            var parser = CreateParser("127.0.0.1");
            var made = parser.TryMakeIpv4AddressLiteral(out string address);
            Assert.IsTrue(made);
            Assert.AreEqual("127.0.0.1", address);
        }
        
        [TestCase("0")]
        [TestCase("A9")]
        [TestCase("ABC")]
        [TestCase("ABCD")]
        [TestCase("1BCD")]
        [TestCase("1BC2")]
        [TestCase("1B2D")]
        [TestCase("1B23")]
        [TestCase("AB23")]
        public void should_make_16bit_number(string input)
        {
            var parser = CreateParser(input);

            var result = parser.TryMake16BitHexNumber(out var hexNumber);

            Assert.True(result);
            Assert.AreEqual(input, hexNumber);
        }
        
        [TestCase("G")]
        [TestCase("A123B")]
        public void should_not_make_16bit_number(string input)
        {
            var parser = CreateParser(input);

            var result = parser.TryMake16BitHexNumber(out var hexNumber);

            Assert.False(result);
        }

        [TestCase("ABCD:EF01:2345:6789:ABCD:EF01:2345:6789")]
        [TestCase("2001:DB8::8:800:200C:417A")]
        [TestCase("FF01::101")]
        [TestCase("::1")]
        [TestCase("::")]
        [TestCase("0:0:0:0:0:0:13.1.68.3")]
        [TestCase("0:0:0:0:0:FFFF:129.144.52.38")]
        [TestCase("::13.1.68.3")]
        [TestCase("::FFFF:129.144.52.38")]
        public void should_make_ipv6_address_literal(string input)
        {
            var parser = CreateParser(input);

            var result = parser.TryMakeIpv6AddressLiteral(out var address);

            Assert.True(result);
            Assert.AreEqual(input, address);
        }

        [TestCase("ABCD:EF01:2345:6789:ABCD:EF01:2345")]
        [TestCase("ABCD:EF01:2345:6789:ABCD:EF01:2345:6789:0")]
        [TestCase("FF01:::101")]
        [TestCase(":::1")]
        [TestCase(":::")]
        public void should_not_make_ip6_address_literal(string input)
        {
            var parser = CreateParser(input);

            var result = parser.TryMakeIpv6AddressLiteral(out var address);

            Assert.False(result);
        }
        
        [TestCase("127.0.0.1")]
        [TestCase("ABCD:EF01:2345:6789:ABCD:EF01:2345:6789")]
        public void should_make_address_literal(string input)
        {
            var parser = CreateParser(input);

            var made = parser.TryMakeAddressLiteral(out var address);

            Assert.True(made);
            Assert.AreEqual(input, address);
        }

        [TestCase(SpfQualifier.Fail, true, "-")]
        [TestCase(SpfQualifier.SoftFail, true, "~")]
        [TestCase(SpfQualifier.Pass, true, "+")]
        [TestCase(SpfQualifier.Neutral, true, "?")]
        [TestCase(SpfQualifier.Neutral, false, "(")]
        public void should_make_qualifier(SpfQualifier expectedQualifier, bool expectedResult, string text)
        {
            var parser = CreateParser(text);
            var result = parser.TryMakeQualifier(out SpfQualifier qualifier);
            
            Assert.AreEqual(expectedQualifier, qualifier);
            Assert.AreEqual(expectedResult, result);
        }

        [TestCase(true, "=")]
        [TestCase(true, ":")]
        [TestCase(true, "/")]
        [TestCase(false, "&")]
        public void should_make_separator(bool expectedResult, string text)
        {
            var parser = CreateParser(text);
            var result = parser.TryMakeSeparator(out string separator);

            Assert.AreEqual(expectedResult, result);

            if (result)
            {
                Assert.AreEqual(text, separator);
            }
        }

        [TestCase(SpfQualifier.Fail, true, "-all")]
        [TestCase(SpfQualifier.SoftFail, true, "~all")]
        [TestCase(SpfQualifier.Pass, true, "+all")]
        [TestCase(SpfQualifier.Neutral, true, "?all")]
        [TestCase(SpfQualifier.Neutral, true, "all")]
        [TestCase(SpfQualifier.Neutral, false, "(all")]
        public void should_make_all(SpfQualifier expectedQualifier, bool expectedResult, string text)
        {
            var parser = CreateParser(text);
            var result = parser.TryMakeAll(out All mechanism, out ParsingError error);
            
            Assert.AreEqual(expectedResult, result);

            if (result)
            {
                Assert.AreEqual(expectedQualifier, mechanism.Qualifier);
            }
            else
            {
                Assert.NotNull(error);
                Assert.AreEqual(error.ErrorType, ParsingErrorType.SyntaxError);
                Assert.AreEqual(error.Source, text);
            }
        }
    }
}