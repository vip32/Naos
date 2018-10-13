namespace Naos.Core.UnitTests.Common
{
    using Naos.Core.Common;
    using Xunit;

    public class SubstringFromTests
    {
        [Fact]
        public void From_All_Positions()
        {
            Assert.Equal("a".SubstringFrom("a"), string.Empty);
            Assert.Equal("a".SubstringFrom("z"), string.Empty);
            Assert.Equal("bbb", "aaa.bbb".SubstringFrom("."));
            Assert.Equal("bb", "aaa.bbb".SubstringFrom("b"));
            Assert.Equal(string.Empty, "aaa.bbb".SubstringFrom("bbb"));
            Assert.Equal(string.Empty, "aaa.bbb".SubstringFrom("z"));
            Assert.Equal("aa.bbb", "aaa.bbb".SubstringFrom("a"));
            Assert.Equal(".bbb", "aaa.bbb".SubstringFrom("aaa"));
        }

        [Fact]
        public void From_Last_Positions()
        {
            Assert.Equal("jpg", "abcdef.jpg".SubstringFrom("."));
            Assert.Equal("jpg.jpg", "abcdef.jpg.jpg".SubstringFrom("."));
            Assert.Equal("jpg", "abcdef.jpg".SubstringFromLast("."));
            Assert.Equal(string.Empty, "jpg".SubstringFromLast("."));
            Assert.Equal("jpg", "abcdef.jpg.jpg".SubstringFromLast("."));
        }
    }
}
