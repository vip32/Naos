namespace Naos.Core.UnitTests.Common
{
    using Naos.Core.Common;
    using Xunit;

    public class EqualsWildcardTests
    {
        [Fact]
        public void Various()
        {
            Assert.True("Test".EqualsWildcard("*"));
            Assert.True("Test".EqualsWildcard("Test"));
            Assert.False("Test1".EqualsWildcard("Test"));
            Assert.False("Test".EqualsWildcard("test", false));
            Assert.True("Test".EqualsWildcard("Test", false));
            Assert.False("Test123".EqualsWildcard("test*", false));
            Assert.True("Test234".EqualsWildcard("Test*", false));
            Assert.True("Test1".EqualsWildcard("Test*"));
            Assert.False("1Test1".EqualsWildcard("Test*"));
            Assert.True("1Test1".EqualsWildcard("*Test*"));
            Assert.True("³[]³}{]}{]}³1Test1³²[²³{[]²³$&%/$&%".EqualsWildcard("*Test*"));
            Assert.True("Te\\asd[\\w]st".EqualsWildcard("Te\\asd[\\w]st"));
        }
    }
}
