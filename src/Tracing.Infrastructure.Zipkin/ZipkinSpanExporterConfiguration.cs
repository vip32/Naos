namespace Naos.Tracing.Infrastructure
{
    public class ZipkinSpanExporterConfiguration
    {
        public bool Enabled { get; set; } = true;

        public string Host { get; set; } //= "http://localhost:9411";

        //public string ServiceName { get; set; } = "Naos";
    }
}
