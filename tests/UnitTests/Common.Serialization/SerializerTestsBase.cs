namespace Naos.Core.UnitTests.Common.Serialization
{
    using System.Collections.Generic;
    using Naos.Core.Common;
    using Xunit;

    public abstract class SerializerTestsBase
    {
        public virtual void CanRoundTripBytes_Test()
        {
            var serializer = this.GetSerializer();
            if(serializer == null)
            {
                return;
            }

            var model = new StubModel
            {
                IntProperty = 1,
                StringProperty = "test",
                ListProperty = new List<int> { 1 },
                ObjectProperty = new StubModel { IntProperty = 1 }
            };

            var bytes = serializer.SerializeToBytes(model);
            var actual = serializer.Deserialize<StubModel>(bytes);
            Assert.Equal(model.IntProperty, actual.IntProperty);
            Assert.Equal(model.StringProperty, actual.StringProperty);
            Assert.Equal(model.ListProperty, actual.ListProperty);

            var text = serializer.SerializeToString(model);
            actual = serializer.Deserialize<StubModel>(text);
            Assert.Equal(model.IntProperty, actual.IntProperty);
            Assert.Equal(model.StringProperty, actual.StringProperty);
            Assert.Equal(model.ListProperty, actual.ListProperty);
            Assert.NotNull(model.ObjectProperty);
            Assert.Equal(1, ((dynamic)model.ObjectProperty).IntProperty);
        }

        public virtual void CanRoundTripString_Test()
        {
            var serializer = this.GetSerializer();
            if(serializer == null)
            {
                return;
            }

            var model = new StubModel
            {
                IntProperty = 1,
                StringProperty = "test",
                ListProperty = new List<int> { 1 },
                ObjectProperty = new StubModel { IntProperty = 1 }
            };

            var text = serializer.SerializeToString(model);
            var actual = serializer.Deserialize<StubModel>(text);
            Assert.Equal(model.IntProperty, actual.IntProperty);
            Assert.Equal(model.StringProperty, actual.StringProperty);
            Assert.Equal(model.ListProperty, actual.ListProperty);
            Assert.NotNull(model.ObjectProperty);
            Assert.Equal(1, ((dynamic)model.ObjectProperty).IntProperty);
        }

        protected virtual ISerializer GetSerializer()
        {
            return null;
        }
    }

#pragma warning disable SA1402 // File may only contain a single class
    public class StubModel
#pragma warning restore SA1402 // File may only contain a single class
    {
        public int IntProperty { get; set; }

        public string StringProperty { get; set; }

        public List<int> ListProperty { get; set; }

        public object ObjectProperty { get; set; }
    }
}