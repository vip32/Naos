namespace Naos.Core.UnitTests.Common
{
    using System.Collections.Generic;
    using Naos.Core.Common;
    using Xunit;

    public class AddOrUpdateTests
    {
        [Fact]
        public void Various()
        {
            var sut = new Dictionary<string, object>();

            sut.AddOrUpdate("key1", "val1");
            sut.AddOrUpdate("key1", "val2");

            Assert.True(sut.ContainsKey("key1"));
            Assert.True(sut["key1"].Equals("val2"));
        }
    }
}
