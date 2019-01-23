namespace Naos.Core.ServiceContext.App.Web
{
    public class ServicePoweredByMiddlewareOptions
    {
        public bool Enabled { get; set; } = true;

        public string HeaderName => "X-PoweredBy";

        public string HeaderValue { get; set; } = "naos";
    }
}
