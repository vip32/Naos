namespace Naos.Core.Operations.Infrastructure.Azure.LogAnalytics
{
    using Naos.Core.Infrastructure.Azure.Ad;

    public class LogAnalyticsConfiguration
    {
        public bool Enabled { get; set; } = true;

        public string WorkspaceId { get; set; } // serilog

        public string AuthenticationId { get; set; } // serilog

        public AuthenticationConfiguration ApiAuthentication { get; set; } // repo

        public string SubscriptionId { get; set; } // repo

        public string ResourceGroupName { get; set; } // repo

        public string WorkspaceName { get; set; } // repo

        public int BufferSize { get; set; } = 10;

        public int BatchSize { get; set; } = 1;
    }
}
