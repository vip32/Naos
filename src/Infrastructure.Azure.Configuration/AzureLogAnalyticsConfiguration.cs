namespace Naos.Core.Infrastructure.Azure.Configuration
{
    public class AzureLogAnalyticsConfiguration
    {
        public bool Enabled { get; set; } = true;

        public string WorkspaceId { get; set; }

        public string AuthenticationId { get; set; }
    }
}
