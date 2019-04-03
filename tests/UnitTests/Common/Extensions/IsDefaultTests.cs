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
            Guid id0 = default;
            var id1 = default(Guid);
            object id2 = default(Guid);
            var dt1 = default(DateTime);

            Assert.True(id0.IsDefault());
            Assert.True(id1.IsDefault());
            Assert.True(id2.IsDefault());
            Assert.True(dt1.IsDefault());
        }

        /// <summary>
        /// Tests the not nullable value with value returns false.
        /// </summary>
        [Fact]
        public void TestNotNullableValueWithValueReturnsFalse()
        {
            var id1 = Guid.NewGuid();
            object id2 = Guid.NewGuid();

            Assert.False(id1.IsDefault());
            Assert.False(id2.IsDefault());
        }

        [Fact]
        public void TestNullableValueWithDefaultReturnsTrue()
        {
            string id0 = default;
            var id1 = default(string);
            object id2 = default(string);

            Assert.True(id0.IsDefault());
            Assert.True(id1.IsDefault());
            Assert.True(id2.IsDefault());
        }

        [Fact]
        public void TestNullableValueWithValueReturnsFalse()
        {
            var id1 = "Test";
            object id2 = "Test";

            Assert.False(id1.IsDefault());
            Assert.False(id2.IsDefault());
        }

        [Fact]
        public void TestIntValueWithValueReturnsFalse()
        {
            var id0 = 1;
            var id1 = 1;
            object id2 = 1;

            Assert.False(id0.IsDefault());
            Assert.False(id1.IsDefault());
            Assert.False(id2.IsDefault());
        }

        [Fact]
        public void TestIntValueWithValueReturnsTrue()
        {
            var id0 = 0;
            var id1 = 0;
            object id2 = 0;

            Assert.True(id0.IsDefault());
            Assert.True(id1.IsDefault());
            Assert.True(id2.IsDefault());
        }
    }
}
