namespace Microsoft.AspNetCore.Builder
{
    using System;
    using System.Linq;
    using EnsureThat;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Hosting.Server.Features;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Naos.Core.App.Correlation.App.Web;
    using Naos.Core.Common;
    using Naos.Core.Discovery.App;

    /// <summary>
    /// Extension methods for the correlation middleware.
    /// </summary>
    public static class ApplicationExtensions
    {
        /// <summary>
        /// Enables correlation/request ids for the API request/responses.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="configuration"></param>
        /// <param name="lifetime"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseNaosDiscovery(this IApplicationBuilder app, IConfiguration configuration, IApplicationLifetime lifetime, string section = "naos:serviceDiscovery")
        {
            EnsureArg.IsNotNull(app, nameof(app));

            return app.UseNaosDiscovery(configuration.GetSection(section).Get<DiscoveryConfiguration>(), lifetime);
        }

        /// <summary>
        /// Enables correlation/request ids for the API request/responses.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="lifetime"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseNaosDiscovery(this IApplicationBuilder app, IApplicationLifetime lifetime)
        {
            EnsureArg.IsNotNull(app, nameof(app));

            return app.UseNaosDiscovery(new DiscoveryConfiguration(), lifetime);
        }

        /// <summary>
        /// Enables correlation/request ids for the API request/responses
        /// </summary>
        /// <param name="app"></param>
        /// <param name="configuration"></param>
        /// <param name="lifetime"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseNaosDiscovery(
            this IApplicationBuilder app,
            DiscoveryConfiguration configuration,
            IApplicationLifetime lifetime)
        {
            EnsureArg.IsNotNull(app, nameof(app));
            EnsureArg.IsNotNull(configuration, nameof(configuration));
            EnsureArg.IsNotNull(lifetime, nameof(lifetime));

            // Determine service address
            var address = configuration.Addresses?.FirstOrDefault();
            if (address.IsNullOrEmpty())
            {
                var features = app.Properties["server.Features"] as FeatureCollection;
                var addressFeature = features?.Get<IServerAddressesFeature>();
                address = addressFeature?.Addresses?.First();
            }

            //var serviceDescriptor = app.ApplicationServices.GetRequiredService<ServiceDescriptor>();

            if (address != null)
            {
                // Register this service (use ServiceDescriptor for more infos)
                var uri = new Uri(address);
                var registration = new ServiceRegistration()
                {
                    Id = $"{AppDomain.CurrentDomain.FriendlyName}-{HashAlgorithm.ComputeHash(uri)}", // TODO: use servicedescriptor for id/name
                    Name = AppDomain.CurrentDomain.FriendlyName,
                    Address = $"{uri.Scheme}://{uri.Host}",
                    Port = uri.Port
                };

                if (app.ApplicationServices.GetRequiredService<IServiceRegistry>() != null)
                {
                    var registry = app.ApplicationServices.GetRequiredService<IServiceRegistry>();
                    registry.Register(registration);

                    lifetime.ApplicationStopping.Register(() =>
                    {
                        registry.DeRegister(registration.Id);
                    });
                }
                else
                {
                    // log warning : no registry resolved
                }
            }
            else
            {
                // log warning : no address
            }

            return app;
        }
    }
}
