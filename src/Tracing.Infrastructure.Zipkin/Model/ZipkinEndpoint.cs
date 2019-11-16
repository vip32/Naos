namespace Naos.Tracing.Infrastructure.Zipkin
{
    using Newtonsoft.Json;

    public class ZipkinEndpoint
    {
        [JsonProperty("serviceName")]
        public string ServiceName { get; set; }

        [JsonProperty("ipv4")]
        public string Ipv4 { get; set; }

        [JsonProperty("ipv6")]
        public string Ipv6 { get; set; }

        [JsonProperty("port")]
        public int Port { get; set; }
    }
}
