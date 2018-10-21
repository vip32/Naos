namespace Naos.Core.Messaging.Infrastructure.Azure.ServiceBus
{
    public class AzureUserCredentials // TODO: move to infra.arm
    {
        public string ClientId { get; internal set; }

        public string ClientSecret { get; internal set; }

        public string TenantId { get; internal set; }

        public string EnvironmentName { get; internal set; }
    }
}