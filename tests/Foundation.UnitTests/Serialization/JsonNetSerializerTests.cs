namespace Naos.Foundation.UnitTests.Serialization
{
    using System.IO;
    using Naos.Foundation;
    using Shouldly;
    using Xunit;

    public class JsonNetSerializerTests : SerializerTestsBase
    {
        [Fact]
        public void CanRoundTripStream_Test()
        {
            // arrange
            var sut = this.GetSerializer();
            var model = new StubModel()
            {
                StringProperty = "abc"
            };

            using (var stream = new MemoryStream())
            {
                // act
                sut.Serialize(model, stream);
                var newModel = sut.Deserialize<StubModel>(stream);

                // assert
                stream.ShouldNotBeNull();
                stream.Length.ShouldBeGreaterThan(0);
                newModel.ShouldNotBeNull();
                newModel.StringProperty.ShouldBe(model.StringProperty);
            }
        }

        [Fact]
        public void CanRoundTripEmptyStream_Test()
        {
            // arrange
            var sut = this.GetSerializer();
            StubModel model = null;

            using (var stream = new MemoryStream())
            {
                // act
                sut.Serialize(model, stream);
                var newModel = sut.Deserialize<StubModel>(stream);

                // assert
                stream.ShouldNotBeNull();
                stream.Length.ShouldBe(0);
                newModel.ShouldBeNull();
            }
        }

        [Fact]
        public override void CanRoundTripBytes_Test()
        {
            base.CanRoundTripBytes_Test();
        }

        [Fact]
        public override void CanRoundTripString_Test()
        {
            base.CanRoundTripString_Test();
        }

        [Fact(Skip = "Skip benchmarks for now")]
        public virtual void Benchmark()
        {
            var summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<JsonNetSerializerBenchmark>();
        }

        protected override ISerializer GetSerializer()
        {
            return new JsonNetSerializer();
        }
    }

#pragma warning disable SA1402 // File may only contain a single class
    public class JsonNetSerializerBenchmark : SerializerBenchmarkBase
#pragma warning restore SA1402 // File may only contain a single class
    {
        protected override ISerializer GetSerializer()
        {
            return new JsonNetSerializer();
        }
    }
}
