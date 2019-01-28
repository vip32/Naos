namespace Naos.Core.ServiceDiscovery.App
{
    /// <summary>
    /// Service discovery configuration.
    /// </summary>
    public class ServiceDiscoveryConfiguration
    {
        public bool Enabled { get; set; } = true;

        public string[] ServiceAddresses { get; set; }

        public string RemoteAddress { get; set; } // optional router address
    }
}
