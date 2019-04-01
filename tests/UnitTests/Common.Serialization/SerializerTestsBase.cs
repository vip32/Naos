namespace Naos.Core.UnitTests.Common.Serialization
{
    using System.Collections.Generic;
    using Naos.Core.Common.Serialization;
    using Xunit;

    public abstract class SerializerTestsBase
    {
        public virtual void CanRoundTripBytes()
        {
            var serializer = this.GetSerializer();
            if (serializer == null)
            {
                return;
            }

            var model = new SerializeModel
            {
                IntProperty = 1,
                StringProperty = "test",
                ListProperty = new List<int> { 1 },
                ObjectProperty = new SerializeModel { IntProperty = 1 }
            };

            byte[] bytes = serializer.SerializeToBytes(model);
            var actual = serializer.Deserialize<SerializeModel>(bytes);
            Assert.Equal(model.IntProperty, actual.IntProperty);
            Assert.Equal(model.StringProperty, actual.StringProperty);
            Assert.Equal(model.ListProperty, actual.ListProperty);

            //string text = serializer.SerializeToString(model);
            //actual = serializer.Deserialize<SerializeModel>(text);
            //Assert.Equal(model.IntProperty, actual.IntProperty);
            //Assert.Equal(model.StringProperty, actual.StringProperty);
            //Assert.Equal(model.ListProperty, actual.ListProperty);
            //Assert.NotNull(model.ObjectProperty);
            //Assert.Equal(1, ((dynamic)model.ObjectProperty).IntProperty);
        }

        public virtual void CanRoundTripString()
        {
            var serializer = this.GetSerializer();
            if (serializer == null)
            {
                return;
            }

            var model = new SerializeModel
            {
                IntProperty = 1,
                StringProperty = "test",
                ListProperty = new List<int> { 1 },
                ObjectProperty = new SerializeModel { IntProperty = 1 }
            };

            string text = serializer.SerializeToString(model);
            var actual = serializer.Deserialize<SerializeModel>(text);
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
    public class SerializeModel
#pragma warning restore SA1402 // File may only contain a single class
    {
        public int IntProperty { get; set; }

        public string StringProperty { get; set; }

        public List<int> ListProperty { get; set; }

        public object ObjectProperty { get; set; }
    }
}