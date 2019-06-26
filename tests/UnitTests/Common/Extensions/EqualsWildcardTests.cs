namespace Naos.Core.UnitTests.Common
{
    using Naos.Foundation;
    using Xunit;

    public class EqualsWildcardTests
    {
        [Fact]
        public void Various()
        {
            Assert.True("Test".EqualsPattern("*"));
            Assert.True("Test".EqualsPattern("Test"));
            Assert.False("Test1".EqualsPattern("Test"));
            Assert.False("Test".EqualsPattern("test", false));
            Assert.True("Test".EqualsPattern("Test", false));
            Assert.True("1234Test".EqualsPattern("*Test"));
            Assert.False("Test123".EqualsPattern("test*", false));
            Assert.True("Test234".EqualsPattern("Test*", false));
            Assert.True("Test1".EqualsPattern("Test*"));
            Assert.False("1Test1".EqualsPattern("Test*"));
            Assert.True("1Test1".EqualsPattern("*Test*"));
            Assert.True("³[]³}{]}{]}³1Test1³²[²³{[]²³$&%/$&%".EqualsPattern("*Test*"));
            Assert.True("Te\\asd[\\w]st".EqualsPattern("Te\\asd[\\w]st"));
        }
    }
}
