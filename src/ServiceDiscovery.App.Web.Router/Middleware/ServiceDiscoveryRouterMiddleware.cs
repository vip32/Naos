namespace Naos.ServiceDiscovery.App.Web.Router
{
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Naos.Foundation;

    public class ServiceDiscoveryRouterMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<ServiceDiscoveryRouterMiddleware> logger;
        private readonly ServiceDiscoveryRouterMiddlewareOptions options;

        public ServiceDiscoveryRouterMiddleware(
            RequestDelegate next,
            ILogger<ServiceDiscoveryRouterMiddleware> logger,
            IOptions<ServiceDiscoveryRouterMiddlewareOptions> options)
        {
            EnsureArg.IsNotNull(next, nameof(next));
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.next = next;
            this.logger = logger;
            this.options = options.Value ?? new ServiceDiscoveryRouterMiddlewareOptions();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await this.next(context).AnyContext();
            // router based on proxy kit https://github.com/damianh/ProxyKit/blob/master/src/Recipes/09_ConsulServiceDisco.cs
        }
    }
}