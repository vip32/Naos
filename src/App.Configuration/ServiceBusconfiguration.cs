namespace Naos.Core.App.Configuration
{
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
