namespace Naos.Core.UnitTests.Common
{
    using Naos.Core.Common;
    using Xunit;

    public class SubstringTillTests
    {
        [Fact]
        public void From_All_Positions()
        {
            Assert.Equal(string.Empty, "a".SubstringTill("a"));
            Assert.Equal("aaa", "aaa.bbb".SubstringTill("."));
            Assert.Equal("aaa.", "aaa.bbb".SubstringTill("b"));
            Assert.Equal("aaa.", "aaa.bbb".SubstringTill("bbb"));
            Assert.Equal("aaa.bbb", "aaa.bbb".SubstringTill("z"));
            Assert.Equal(string.Empty, "aaa.bbb".SubstringTill("a"));
            Assert.Equal(string.Empty, "aaa.bbb".SubstringTill("aaa"));
        }

        [Fact]
        public void From_Last_Positions()
        {
            Assert.Equal("abcdef", "abcdef.jpg".SubstringTill("."));
            Assert.Equal("abcdef", "abcdef.jpg.jpg".SubstringTill("."));
            Assert.Equal("abcdef", "abcdef.jpg".SubstringTillLast("."));
            Assert.Equal("abcdef.jpg", "abcdef.jpg.jpg".SubstringTillLast("."));
        }
    }
}
