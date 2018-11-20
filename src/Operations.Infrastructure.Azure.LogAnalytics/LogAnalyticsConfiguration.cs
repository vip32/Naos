namespace Naos.Core.Operations.Infrastructure.Azure.LogAnalytics
{
    using Naos.Core.Infrastructure.Azure.Ad;

    public class LogAnalyticsConfiguration
    {
        public bool Enabled { get; set; } = true;

        public string WorkspaceId { get; set; }

        public string AuthenticationId { get; set; }

        public AuthenticationConfiguration ApiAuthentication { get; set; }
    }
}
