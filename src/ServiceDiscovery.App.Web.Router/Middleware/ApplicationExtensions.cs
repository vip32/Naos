namespace Microsoft.AspNetCore.Builder
{
    using System;
    using System.Linq;
    using EnsureThat;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Naos.Core.ServiceDiscovery.App;
    using Naos.Core.ServiceDiscovery.App.Web;
    using Naos.Core.ServiceDiscovery.App.Web.Router;
    using ProxyKit;

    /// <summary>
    /// Extension methods for the service discovery router middleware.
    /// </summary>
    public static class ApplicationExtensions
    {
        /// <summary>
        /// Enables the service discovery router
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseNaosServiceDiscoveryRouter(this IApplicationBuilder app)
        {
            EnsureArg.IsNotNull(app, nameof(app));

            return app.UseNaosServiceDiscoveryRouter(new ServiceDiscoveryRouterMiddlewareOptions());
        }

        /// <summary>
        /// Enables the service discovery router
        /// </summary>
        /// <param name="app"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseNaosServiceDiscoveryRouter(this IApplicationBuilder app, ServiceDiscoveryRouterMiddlewareOptions options)
        {
            EnsureArg.IsNotNull(app, nameof(app));
            EnsureArg.IsNotNull(options, nameof(options));

            app.RunProxy(async context =>
            {
                var client = context.RequestServices.GetRequiredService<IServiceRegistryClient>();

                // read headers for service/tag
                var isFoundName = context.Request.Headers.TryGetValue(ServiceDiscoveryRouterHeaders.ServiceName, out var serviceName);
                var isFoundTag = context.Request.Headers.TryGetValue(ServiceDiscoveryRouterHeaders.ServiceTag, out var serviceTag);

                var registrations = await client.ServicesAsync(serviceName, serviceTag).ConfigureAwait(false);

                // TODO: how is api/registrations NOT forwarded? based on missing router headers?
                // TODO: round robin https://github.com/damianh/ProxyKit/blob/master/src/Recipes/05_RoundRobin.cs
                var upstreamHost = new Uri($"https://{registrations.FirstOrDefault()?.Address}:{registrations.FirstOrDefault()?.Port}");
                return await context
                    .ForwardTo(upstreamHost)
                    .AddXForwardedHeaders()
                    .Send();
            });

            //return app.UseMiddleware<ServiceDiscoveryRouterMiddleware>(Options.Create(options));
            return app;
        }
    }
}
