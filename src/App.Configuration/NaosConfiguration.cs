namespace Naos.Core.App.Configuration
{
    public class NaosConfiguration
    {
        public Appconfiguration App { get; set; }

        public SecretsConfiguration Secrets { get; set; }

        public MessagingConfiguration Messaging { get; set; }

        public class Appconfiguration
        {
            public CosmosDbConfiguration CosmosDb { get; set; }
        }

        public class SecretsConfiguration
        {
            public KeyVaultConfiguration Vault { get; set; }
        }

        public class KeyVaultConfiguration
        {
            public bool Enabled { get; set; }

            public string Name { get; set; }

            public string ClientId { get; set; }

            public string ClientSecret { get; set; }
        }

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

        public class MessagingConfiguration
        {
            public ServiceBusconfiguration ServiceBus { get; set; }
        }

        public class ServiceBusconfiguration
        {
            public bool Enabled { get; set; }

            public string ResourceGroup { get; set; }

            public string NamespaceName { get; set; }

            public string ClientId { get; set; }

            public string ClientSecret { get; set; }

            public string SubscriptionId { get; set; }

            public string TenantId { get; set; }

            public string ConnectionString { get; set; }

            public string EntityPath { get; internal set; }
        }
    }
}
