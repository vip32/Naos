namespace Naos.Foundation.UnitTests.Serialization
{
    using System.Collections.Generic;
    using System.IO;
    using Naos.Foundation;
    using Shouldly;
    using Xunit;

    public abstract class SerializerTestsBase
    {
        public virtual void CanRoundTripStream_Test()
        {
            // arrange
            var sut = this.GetSerializer();
            var value = new StubModel()
            {
                StringProperty = "abc"
            };

            using (var stream = new MemoryStream())
            {
                // act
                sut.Serialize(value, stream);
                var newModel = sut.Deserialize<StubModel>(stream);

                // assert
                stream.ShouldNotBeNull();
                stream.Length.ShouldBeGreaterThan(0);
                newModel.ShouldNotBeNull();
                newModel.StringProperty.ShouldBe(value.StringProperty);
            }
        }

        public virtual void CanRoundTripEmptyStream_Test()
        {
            // arrange
            var sut = this.GetSerializer();
            StubModel value = null;

            using (var stream = new MemoryStream())
            {
                // act
                sut.Serialize(value, stream);
                var newModel = sut.Deserialize<StubModel>(stream);

                // assert
                stream.ShouldNotBeNull();
                stream.Length.ShouldBe(0);
                newModel.ShouldBeNull();
            }
        }

        public virtual void CanRoundTripBytes_Test()
        {
            var serializer = this.GetSerializer();
            if (serializer == null)
            {
                return;
            }

            var value = new StubModel
            {
                IntProperty = 1,
                StringProperty = "test",
                ListProperty = new List<int> { 1 },
                ObjectProperty = new StubModel { IntProperty = 1 }
            };

            var bytes = serializer.SerializeToBytes(value);
            var actual = serializer.Deserialize<StubModel>(bytes);
            Assert.Equal(value.IntProperty, actual.IntProperty);
            Assert.Equal(value.StringProperty, actual.StringProperty);
            Assert.Equal(value.ListProperty, actual.ListProperty);

            var text = serializer.SerializeToString(value);
            actual = serializer.Deserialize<StubModel>(text);
            Assert.Equal(value.IntProperty, actual.IntProperty);
            Assert.Equal(value.StringProperty, actual.StringProperty);
            Assert.Equal(value.ListProperty, actual.ListProperty);
            Assert.NotNull(value.ObjectProperty);
            Assert.Equal(1, ((dynamic)value.ObjectProperty).IntProperty);
        }

        public virtual void CanRoundTripString_Test()
        {
            var serializer = this.GetSerializer();
            if (serializer == null)
            {
                return;
            }

            var value = new StubModel
            {
                IntProperty = 1,
                StringProperty = "test",
                ListProperty = new List<int> { 1 },
                ObjectProperty = new StubModel { IntProperty = 1 }
            };

            var text = serializer.SerializeToString(value);
            var actual = serializer.Deserialize<StubModel>(text);
            Assert.Equal(value.IntProperty, actual.IntProperty);
            Assert.Equal(value.StringProperty, actual.StringProperty);
            Assert.Equal(value.ListProperty, actual.ListProperty);
            Assert.NotNull(value.ObjectProperty);
            Assert.Equal(1, ((dynamic)value.ObjectProperty).IntProperty);
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