namespace Naos.Core.UnitTests.Common.Serialization
{
    using System.Collections.Generic;
    using BenchmarkDotNet.Attributes;
    using Naos.Core.Common;

    [MemoryDiagnoser]
    [ShortRunJob]
    public abstract class SerializerBenchmarkBase
    {
        private readonly StubModel data = new StubModel
        {
            IntProperty = 1,
            StringProperty = "test",
            ListProperty = new List<int> { 1 },
            ObjectProperty = new StubModel { IntProperty = 1 }
        };

        private ISerializer serializer;

        private byte[] serializedData;

        [GlobalSetup]
        public void Setup()
        {
            this.serializer = this.GetSerializer();
            this.serializedData = this.serializer.SerializeToBytes(this.data);
        }

        [Benchmark]
        public byte[] Serialize()
        {
            return this.serializer.SerializeToBytes(this.data);
        }

        [Benchmark]
        public StubModel Deserialize()
        {
            return this.serializer.Deserialize<StubModel>(this.serializedData);
        }

        [Benchmark]
        public StubModel RoundTrip()
        {
            var serializedData = this.serializer.SerializeToBytes(this.data);
            return this.serializer.Deserialize<StubModel>(serializedData);
        }

        protected abstract ISerializer GetSerializer();
    }
}