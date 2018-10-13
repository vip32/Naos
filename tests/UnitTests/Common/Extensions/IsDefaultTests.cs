namespace Naos.Core.UnitTests.Common
{
    using System;
    using Naos.Core.Common;
    using Xunit;

    public partial class IsDefaultTests
    {
        /// <summary>
        /// Tests the not nullable value with default returns true.
        /// </summary>
        [Fact]
        public void TestNotNullableValueWithDefaultReturnsTrue()
        {
            var id = default(Guid);

            Assert.True(id.IsDefault());
        }

        /// <summary>
        /// Tests the not nullable value with value returns false.
        /// </summary>
        [Fact]
        public void TestNotNullableValueWithValueReturnsFalse()
        {
            var id = Guid.NewGuid();

            Assert.False(id.IsDefault());
        }

        [Fact]
        public void TestNullableValueWithDefaultReturnsTrue()
        {
            var id = default(string);

            Assert.True(id.IsDefault());
        }

        [Fact]
        public void TestNullableValueWithValueReturnsFalse()
        {
            var id = "Test";

            Assert.False(id.IsDefault());
        }
    }
}
