namespace Naos.Tracing.Infrastructure.Zipkin
{
    using Newtonsoft.Json;

    public class ZipkinAnnotation
    {
        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
