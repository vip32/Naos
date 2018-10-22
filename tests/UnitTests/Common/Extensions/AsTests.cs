namespace Naos.Core.UnitTests.Common
{
    using Naos.Core.Common;
    using Shouldly;
    using Xunit;

    public class AsTests
    {
        [Fact]
        public void As_Test()
        {
            var obj = (object)new AsTests();
            obj.As<AsTests>().ShouldNotBe(null);

            obj = null;
            obj.As<AsTests>().ShouldBe(null);
        }
    }
}
