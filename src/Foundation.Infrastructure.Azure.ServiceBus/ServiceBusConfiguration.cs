namespace Naos.Foundation.Infrastructure
{
    public class ServiceBusConfiguration
    {
        public bool Enabled { get; set; }

        public string ConnectionString { get; set; }

        public string EntityPath { get; set; }

        // TODO: move below to Infra.Arm

        public string ResourceGroup { get; set; }

        public string NamespaceName { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string SubscriptionId { get; set; }

        public string TenantId { get; set; }
    }
}