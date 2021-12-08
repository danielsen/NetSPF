using NetSPF.Common;
using NUnit.Framework;

namespace NetSPF.Tests.Unit.Common
{
    [TestFixture]
    public class EnumParserTests
    {
        [TestCase("+", SpfQualifier.Pass, true)]
        [TestCase("-", SpfQualifier.Fail, true)]
        [TestCase("~", SpfQualifier.SoftFail, true)]
        [TestCase("?", SpfQualifier.Neutral, true)]
        [TestCase("*", null, false)]
        public void should_parse_enum_by_mapped_text(string text, SpfQualifier expectedQualifier, bool expectedResult)
        {
            var result = EnumParser.TryParse(text, out SpfQualifier qualifier);
            
            Assert.AreEqual(expectedResult, result);
            Assert.AreEqual(expectedQualifier, qualifier);
        }
    }
}