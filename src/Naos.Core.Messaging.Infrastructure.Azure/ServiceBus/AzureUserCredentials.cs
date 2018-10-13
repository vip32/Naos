namespace Naos.Core.Messaging.Infrastructure.Azure
{
    public class AzureUserCredentials
    {
        public string ClientId { get; internal set; }

        public string ClientSecret { get; internal set; }

        public string TenantId { get; internal set; }

        public string EnvironmentName { get; internal set; }
    }
}