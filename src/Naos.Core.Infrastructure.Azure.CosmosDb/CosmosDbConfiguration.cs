namespace Naos.Core.Infrastructure.Azure.CosmosDb
{
    public class CosmosDbConfiguration
    {
        public string DatabaseId { get; set; }

        public string ServiceEndpointUri { get; set; }

        public string AuthKeyOrResourceToken { get; set; }

        public string CollectionName { get; set; }

        public bool IsMasterCollection { get; set; }

        public string CollectionPartitionKey { get; set; }

        public int CollectionOfferThroughput { get; set; }
    }
}
