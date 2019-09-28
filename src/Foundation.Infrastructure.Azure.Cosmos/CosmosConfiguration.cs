namespace Naos.Foundation.Infrastructure
{
    public class CosmosConfiguration
    {
        public string DatabaseId { get; set; }

        public string ConnectionString { get; set; }

        public string ServiceEndpointUri { get; set; }

        public string AuthKeyOrResourceToken { get; set; }

        public string CollectionId { get; set; }

        public bool IsMasterCollection { get; set; } = true;

        public string CollectionPartitionKey { get; set; }

        public int CollectionOfferThroughput { get; set; } = 400;
    }
}
