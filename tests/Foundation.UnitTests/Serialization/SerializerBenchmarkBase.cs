namespace Naos.Foundation.UnitTests.Serialization
{
    using System.Collections.Generic;
    using BenchmarkDotNet.Attributes;
    using Naos.Foundation;

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
            var data = this.serializer.SerializeToBytes(this.data);
            return this.serializer.Deserialize<StubModel>(data);
        }

        protected abstract ISerializer GetSerializer();
    }
}