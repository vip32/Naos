namespace Naos.Tracing.Infrastructure.Mongo
{
    public class MongoTracingConfiguration
    {
        public bool Enabled { get; set; } = true;

        public string ConnectionString { get; set; } = "mongodb://localhost:27017/naos_operations?connectTimeoutMS=300000";

        public string DatabaseName { get; set; } = "master";

        public string CollectionName { get; set; } = "LogEvents";

        public long? CappedMaxSizeMb { get; set; }

        public long? CappedMaxDocuments { get; set; }
    }
}