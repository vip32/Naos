namespace Naos.ServiceDiscovery.App
{
    using Newtonsoft.Json;

    public class ServiceRegistration // https://cecilphillip.com/using-consul-for-service-discovery-with-asp-net-core/
    {
        public string Id { get; set; }

        public bool Enabled { get; set; } = true;

        public string Name { get; set; }

        public string Address { get; set; }

        public int Port { get; set; }

        public string[] Tags { get; set; }

        public ServiceRegistrationCheck Check { get; set; } = new ServiceRegistrationCheck();

        [JsonIgnore]
        public string FullAddress => $"{this.Address}:{this.Port}".TrimEnd(':');
    }
}
