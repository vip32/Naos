namespace Naos.Core.UnitTests.Common.Serialization
{
    using Naos.Core.Common;
    using Xunit;

    public class HexSerializerTests : SerializerTestsBase
    {
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
            var summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<HexSerializerBenchmark>();
        }

        protected override ISerializer GetSerializer()
        {
            return new HexSerializer();
        }
    }

#pragma warning disable SA1402 // File may only contain a single class
    public class HexSerializerBenchmark : SerializerBenchmarkBase
#pragma warning restore SA1402 // File may only contain a single class
    {
        protected override ISerializer GetSerializer()
        {
            return new HexSerializer();
        }
    }
}
