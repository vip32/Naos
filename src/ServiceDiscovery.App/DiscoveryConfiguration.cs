namespace Naos.Core.ServiceDiscovery.App
{
    /// <summary>
    /// Service discovery configuration.
    /// </summary>
    public class DiscoveryConfiguration
    {
        public bool Enabled { get; set; }

        public string[] Addresses { get; set; }
    }
}
