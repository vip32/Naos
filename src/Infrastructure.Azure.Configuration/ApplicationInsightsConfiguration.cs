namespace Naos.Core.Infrastructure.Azure.Configuration
{
    public class ApplicationInsightsConfiguration
    {
        public bool Enabled { get; set; } = true;

        public string ApplicationKey { get; set; }
    }
}
