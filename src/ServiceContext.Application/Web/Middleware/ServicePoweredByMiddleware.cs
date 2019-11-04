namespace Naos.ServiceContext.Application.Web
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;

    public class ServicePoweredByMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ServicePoweredByMiddlewareOptions options;

        public ServicePoweredByMiddleware(
            RequestDelegate next,
            IOptions<ServicePoweredByMiddlewareOptions> options)
        {
            this.next = next;
            this.options = options.Value ?? new ServicePoweredByMiddlewareOptions();
        }

        public Task Invoke(HttpContext httpContext)
        {
            if (this.options.Enabled)
            {
                httpContext.Response.Headers[this.options.HeaderName] = this.options.HeaderValue;
            }

            return this.next.Invoke(httpContext);
        }
    }
}
