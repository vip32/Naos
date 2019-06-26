namespace Naos.Core.UnitTests.Common
{
    using System.Collections.Generic;
    using Naos.Foundation;
    using Xunit;

    public class EmptyToNulllTests
    {
        [Fact]
        public void NullListReturnsNull()
        {
            List<string> list = null;

            Assert.Null(list.EmptyToNull());
        }

        [Fact]
        public void EmptyListReturnsNull()
        {
            var list = new List<string>();

            Assert.Null(list.EmptyToNull());
        }

        [Fact]
        public void NonEmptyListReturnsNotNull()
        {
            var list = new List<string>
            {
                "test123",
                "test456"
            };

            Assert.NotNull(list.EmptyToNull());
        }
    }
}
