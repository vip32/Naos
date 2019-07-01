namespace Naos.Foundation.UnitTests.Serialization
{
    using Naos.Foundation;
    using Xunit;

    public class Base64SerializerTests : SerializerTestsBase
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
            var summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<Base64SerializerBenchmark>();
        }

        protected override ISerializer GetSerializer()
        {
            return new Base64Serializer();
        }
    }

#pragma warning disable SA1402 // File may only contain a single class
    public class Base64SerializerBenchmark : SerializerBenchmarkBase
#pragma warning restore SA1402 // File may only contain a single class
    {
        protected override ISerializer GetSerializer()
        {
            return new Base64Serializer();
        }
    }
}
