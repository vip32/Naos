namespace Microsoft.AspNetCore.Builder
{
    using System;
    using System.Linq;
    using EnsureThat;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Naos.Core.Common;
    using Naos.Core.Common.Web;
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

            app.RunProxy("/router", async context =>
            {
                var logger = app.ApplicationServices.GetRequiredService<ILogger>();
                var client = app.ApplicationServices.GetRequiredService<IServiceRegistryClient>();

                // read headers for service/tag
                var isFoundName = context.Request.Headers.TryGetValue(ServiceDiscoveryRouterHeaders.ServiceName, out var serviceName);
                var isFoundTag = context.Request.Headers.TryGetValue(ServiceDiscoveryRouterHeaders.ServiceTag, out var serviceTag);

                if (!isFoundName && !isFoundTag)
                {
                    throw new NaosException($"Router cannot find a service registration based on the provided information (serviceName={serviceName}, serviceTag={serviceTag})");
                    // = 400 bad request (router headers missing)
                }

                var registrations = await client.ServicesAsync(serviceName, serviceTag).ConfigureAwait(false);
                var registration = registrations.FirstOrDefault(); // todo: do some kind random/roundrobin

                if (registration == null)
                {
                    throw new NaosException($"Router cannot find a service registration based on the provided information (serviceName={serviceName}, serviceTag={serviceTag})");
                    // = 404? 502? https://errorcodespro.com/what-is-http-error-502-bad-gateway/
                }

                // TODO: how is api/registrations NOT forwarded? based on missing router headers?
                // TODO: round robin https://github.com/damianh/ProxyKit/blob/master/src/Recipes/05_RoundRobin.cs
                var upstreamHost = new Uri($"{registration.Address}:{registration.Port}");
                logger.LogInformation($"{{LogKey}} router {{Url}} >> {{Host}} (service={{ServiceName}}, tag={serviceTag})", LogEventKeys.ServiceDiscovery, context.Request.Uri(), upstreamHost, registration.Name);
                return await context
                    .ForwardTo(upstreamHost)
                    .Log(logger)
                    .CopyXForwardedHeaders() // copies the headers from the incoming requests
                    .AddXForwardedHeaders() // adds the current proxy proto/host/for/pathbase to the X-Forwarded headers
                    .Send();
            });

            //return app.UseMiddleware<ServiceDiscoveryRouterMiddleware>(Options.Create(options));
            return app;
        }
    }

#pragma warning disable SA1402 // File may only contain a single class
    public static class RouterExtensions
#pragma warning restore SA1402 // File may only contain a single class
    {
        public static ForwardContext Log(this ForwardContext forwardContext, ILogger logger)
        {
            logger.LogInformation($"upstream url: {forwardContext.UpstreamRequest.RequestUri}");

            return forwardContext;
        }
    }
}
