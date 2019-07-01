namespace Naos.Foundation.UnitTests.Extensions
{
    using System.Collections.Generic;
    using Naos.Foundation;
    using Xunit;

    public class AddOrUpdateTests
    {
        [Fact]
        public void AddKeysAndValues()
        {
            var sut = new Dictionary<string, object>();

            sut.AddOrUpdate("key1", "val1");
            sut.AddOrUpdate("key1", "val2");

            Assert.True(sut.ContainsKey("key1"));
            Assert.True(sut["key1"].Equals("val2"));
        }

        [Fact]
        public void AddDictionary()
        {
            var sut = new Dictionary<string, object>();
            sut.AddOrUpdate("key1", "val1");
            sut.AddOrUpdate("key1", "val2");

            var items = new Dictionary<string, object>();
            items.AddOrUpdate("key3", "val4");
            items.AddOrUpdate("key4", "val4");

            sut.AddOrUpdate(items);

            Assert.True(sut.ContainsKey("key4"));
            Assert.True(sut["key4"].Equals("val4"));
        }
    }
}
