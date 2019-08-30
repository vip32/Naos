namespace Naos.Foundation.UnitTests.Extensions
{
    using Naos.Foundation;
    using Shouldly;
    using Xunit;

    public partial class IsTests
    {
        /// <summary>
        /// Tests the not nullable value with default returns true.
        /// </summary>
        [Fact]
        public void TestVariousReturnTrue()
        {
            object id0 = true;
            object id1 = 12.33m;
            object id2 = "test";
            var id21 = "test";
            object id3 = double.MaxValue;

            Assert.True(id0.Is<bool>());
            Assert.True(id1.Is<decimal>());
            Assert.True(id2.Is<string>());
            Assert.True(id21.Is<string>());
            Assert.True(id3.Is<double>());
        }
    }
}
