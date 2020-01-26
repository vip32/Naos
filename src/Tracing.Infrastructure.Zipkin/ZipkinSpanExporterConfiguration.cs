namespace Naos.Tracing.Infrastructure
{
    public class ZipkinSpanExporterConfiguration
    {
        public bool Enabled { get; set; } = true;

        public string Endpoint { get; set; } = "http://localhost:9411/api/v2/spans";

        //public string ServiceName { get; set; } = "Naos";
    }
}
