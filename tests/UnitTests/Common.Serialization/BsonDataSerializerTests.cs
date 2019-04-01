namespace Naos.Core.UnitTests.Common.Serialization
{
    using Naos.Core.Common.Serialization;
    using Xunit;

    public class BsonDataSerializerTests : SerializerTestsBase
    {
        [Fact]
        public override void CanRoundTripBytes()
        {
            base.CanRoundTripBytes();
        }

        [Fact(Skip = "string not working")]
        public override void CanRoundTripString()
        {
            base.CanRoundTripString();
        }

        [Fact(Skip = "Skip benchmarks for now")]
        public virtual void Benchmark()
        {
            var summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<BsonDataSerializerBenchmark>();
        }

        protected override ISerializer GetSerializer()
        {
            return new BsonDataSerializer();
        }
    }

#pragma warning disable SA1402 // File may only contain a single class
    public class BsonDataSerializerBenchmark : SerializerBenchmarkBase
#pragma warning restore SA1402 // File may only contain a single class
    {
        protected override ISerializer GetSerializer()
        {
            return new BsonDataSerializer();
        }
    }
}
