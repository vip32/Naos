namespace Naos.Foundation.UnitTests.Serialization
{
    using System.IO;
    using Naos.Foundation;
    using Shouldly;
    using Xunit;

    public class JsonNetSerializerTests : SerializerTestsBase
    {
        [Fact]
        public override void CanRoundTripStream_Test()
        {
            base.CanRoundTripStream_Test();
        }

        [Fact]
        public override void CanRoundTripEmptyStream_Test()
        {
            base.CanRoundTripEmptyStream_Test();
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
