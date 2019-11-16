namespace Naos.Tracing.Infrastructure.Zipkin
{
    public class ZipkinSpanExporterOptions
    {
        public string Endpoint { get; set; } = "http://localhost:9411/api/v2/spans";

        //public string ServiceName { get; set; } = "Naos";
    }
}
