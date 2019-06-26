namespace Naos.Core.UnitTests.Common
{
    using Naos.Foundation;
    using Xunit;

    public partial class IsDefaultTests
    {
        [Fact]
        public void Various()
        {
            const string test = null;
            Assert.False(test.ToBool());
            Assert.False(default(string).ToBool());
            Assert.True(default(string).ToBool(true));
            Assert.True("True".ToBool());
            Assert.True("true".ToBool());
            Assert.True("1".ToBool());
            Assert.False("abc".ToBool());
            Assert.False("False".ToBool());
            Assert.False("false".ToBool());
            Assert.False("0".ToBool());
        }
    }
}
