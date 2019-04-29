namespace Microsoft.AspNetCore.Builder
{
    using System;
    using System.Linq;
    using EnsureThat;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Common.Web;
    using Naos.Core.ServiceDiscovery.App.Web;
    using Naos.Core.ServiceDiscovery.App.Web.Router;
    using ProxyKit;

    /// <summary>
    /// Extension methods for the service discovery router middleware.
    /// </summary>
    public static class ApplicationExtensions
    {
        /// <summary>
        /// Enables the service discovery router.
        /// </summary>
        /// <param name="naosOptions"></param>
        /// <returns></returns>
        public static NaosApplicationContextOptions UseServiceDiscoveryRouter(this NaosApplicationContextOptions naosOptions)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));

            return naosOptions.UseNaosServiceDiscoveryRouter(new ServiceDiscoveryRouterMiddlewareOptions());
        }

        /// <summary>
        /// Enables the service discovery router.
        /// </summary>
        /// <param name="naosOptions"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static NaosApplicationContextOptions UseNaosServiceDiscoveryRouter(
            this NaosApplicationContextOptions naosOptions,
            ServiceDiscoveryRouterMiddlewareOptions options)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));
            EnsureArg.IsNotNull(options, nameof(options));

            naosOptions.Context.Application.RunProxy("/api/servicediscovery/router/proxy", async context =>
            {
                var logger = naosOptions.Context.Application.ApplicationServices.GetRequiredService<ILogger>();
                var registryClient = naosOptions.Context.Application.ApplicationServices.GetRequiredService<RouterContext>().RegistryClient;

                // read headers for service/tag
                var isFoundName = context.Request.Headers.TryGetValue(ServiceDiscoveryRouterHeaders.ServiceName, out var serviceName);
                var isFoundTag = context.Request.Headers.TryGetValue(ServiceDiscoveryRouterHeaders.ServiceTag, out var serviceTag);

                if(!isFoundName && !isFoundTag)
                {
                    context.Response.StatusCode = 400;
                    throw new NaosException($"Router cannot find a service registration based on the provided information (serviceName={serviceName}, serviceTag={serviceTag})");
                    // = 400 bad request (router headers missing)
                }

                var registrations = await registryClient.RegistrationsAsync(serviceName, serviceTag).AnyContext();
                var registration = registrations.FirstOrDefault(); // todo: do some kind random/roundrobin

                if(registration == null)
                {
                    context.Response.StatusCode = 502;
                    throw new NaosException($"Router cannot find a service registration based on the provided information (serviceName={serviceName}, serviceTag={serviceTag})");
                    // = 404? 502? https://errorcodespro.com/what-is-http-error-502-bad-gateway/
                }

                // TODO: how is api/registrations NOT forwarded? based on missing router headers?
                // TODO: round robin https://github.com/damianh/ProxyKit/blob/master/src/Recipes/05_RoundRobin.cs
                var upstreamHost = new Uri($"{registration.Address}:{registration.Port}");
                logger.LogInformation($"{{LogKey:l}} router {{Url}} >> {{Host}} (service={{ServiceName}}, tag={serviceTag})", LogKeys.ServiceDiscovery, context.Request.Uri(), upstreamHost, registration.Name);
                return await context
                    .ForwardTo(upstreamHost)
                    .Log(logger)
                    .CopyXForwardedHeaders() // copies the headers from the incoming requests
                    .AddXForwardedHeaders() // adds the current proxy proto/host/for/pathbase to the X-Forwarded headers
                    .Send();
            });

            naosOptions.Context.Messages.Add($"{LogKeys.Startup} naos application builder: job scheduling added"); // TODO: list available commands/handlers

            //return app.UseMiddleware<ServiceDiscoveryRouterMiddleware>(Options.Create(options));
            return naosOptions;
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
