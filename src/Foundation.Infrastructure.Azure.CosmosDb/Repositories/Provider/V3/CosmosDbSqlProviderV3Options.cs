namespace Naos.Foundation.Infrastructure
{
    using Microsoft.Azure.Cosmos;

    public class CosmosDbSqlProviderV3Options : BaseOptions
    {
        public CosmosClient Client { get; set; }

        public string ConnectionString { get; set; }

        public string AccountEndPoint { get; set; }

        public string AccountKey { get; set; }

        public string Database { get; set; } = "master";

        public string Container { get; set; }

        public string PartitionKeyPath { get; set; }

        public int? ThroughPut { get; set; } = 400;
    }
}