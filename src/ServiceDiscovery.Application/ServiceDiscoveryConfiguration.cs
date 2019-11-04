namespace Naos.ServiceDiscovery.Application
{
    /// <summary>
    /// Service discovery configuration.
    /// </summary>
    public class ServiceDiscoveryConfiguration
    {
        public bool Enabled { get; set; } = true;

        public string[] ServiceAddresses { get; set; }

        public bool RouterEnabled { get; set; } = false;

        public string RouterAddress { get; set; } // optional router address

        public string RouterPath { get; set; }
    }
}
