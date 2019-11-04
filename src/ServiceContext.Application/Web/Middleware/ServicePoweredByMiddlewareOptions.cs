namespace Naos.ServiceContext.Application.Web
{
    public class ServicePoweredByMiddlewareOptions
    {
        public bool Enabled { get; set; } = true;

        public string HeaderName => "X-PoweredBy";

        public string HeaderValue { get; set; } = "naos";
    }
}
