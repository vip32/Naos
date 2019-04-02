namespace Naos.Core.UnitTests.Common.Serialization
{
    using System.Collections.Generic;
    using BenchmarkDotNet.Attributes;
    using Naos.Core.Common;

    [MemoryDiagnoser]
    [ShortRunJob]
    public abstract class SerializerBenchmarkBase
    {
        private readonly SerializeModel data = new SerializeModel
        {
            IntProperty = 1,
            StringProperty = "test",
            ListProperty = new List<int> { 1 },
            ObjectProperty = new SerializeModel { IntProperty = 1 }
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
        public SerializeModel Deserialize()
        {
            return this.serializer.Deserialize<SerializeModel>(this.serializedData);
        }

        [Benchmark]
        public SerializeModel RoundTrip()
        {
            byte[] serializedData = this.serializer.SerializeToBytes(this.data);
            return this.serializer.Deserialize<SerializeModel>(serializedData);
        }

        protected abstract ISerializer GetSerializer();
    }
}