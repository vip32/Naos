namespace Microsoft.AspNetCore.Builder
{
    using System;
    using System.Linq;
    using EnsureThat;
    using Microsoft.AspNetCore.Hosting.Server.Features;
    using Microsoft.AspNetCore.Http.Features;
    using Naos.Core.App.Correlation.App.Web;
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
        /// <returns></returns>
        public static IApplicationBuilder UseNaosDiscovery(this IApplicationBuilder app)
        {
            EnsureArg.IsNotNull(app, nameof(app));

            return app.UseNaosDiscovery(new DiscoveryOptions());
        }

        /// <summary>
        /// Enables correlation/request ids for the API request/responses
        /// </summary>
        /// <param name="app"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseNaosDiscovery(this IApplicationBuilder app, DiscoveryOptions options)
        {
            EnsureArg.IsNotNull(app, nameof(app));
            EnsureArg.IsNotNull(options, nameof(options));

            // Get server IP address
            var features = app.Properties["server.Features"] as FeatureCollection;
            var addressFeature = features?.Get<IServerAddressesFeature>();
            var address = addressFeature?.Addresses?.First(); // or get from DiscoveryOptions.Addresses

            if (address != null)
            {
                // Register service
                var uri = new Uri(address);
                var registration = new ServiceRegistration()
                {
                    Id = $"ServiceName-{uri.Port}",
                    Name = "ServiceName",
                    Address = $"{uri.Scheme}://{uri.Host}",
                    Port = uri.Port,
                    Tags = new[] { "Students", "Courses", "School" }
                };

                //if (app.ApplicationServices.GetService(typeof(IServiceRegistry)) == null)
                //{
                //    throw new InvalidOperationException($"Unable to find the required services. You must call the {nameof(Microsoft.Extensions.DependencyInjection.ServiceRegistrations.AddNaosDiscoveryLocal)} method in ConfigureServices in the application startup code.");
                //}

                if (app.ApplicationServices.GetService(typeof(IServiceRegistry)) is IServiceRegistry registry) // todo: use generics and save the cast
                {
                    registry.Register(registration);
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
