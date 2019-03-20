namespace Naos.Core.Infrastructure.Azure.CosmosDb
{
    using Microsoft.Azure.Cosmos;
    using Naos.Core.Common;

    public class CosmosDbSqlProviderV3Options : BaseOptions
    {
        public CosmosClient Client { get; set; }

        public string ConnectionString { get; set; }

        public string AccountEndPoint { get; set; }

        public string AccountKey { get; set; }

        public string Database { get; set; } = "master";

        public string Container { get; set; }

        public string PartitionKeyPath { get; set; } = "/Discriminator";

        public int? ThroughPut { get; set; } = 400;
    }
}