namespace Microsoft.Extensions.DependencyInjection
{
    using System.Diagnostics.CodeAnalysis;
    using EnsureThat;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Logging;
    using Naos.Configuration.Application;
    using Naos.Foundation;
    using Naos.ServiceContext.Application.Web;

    /// <summary>
    /// Extensions on the <see cref="IServiceCollection"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class NaosExtensions
    {
        /// <summary>
        /// Adds required services to support the service context functionality.
        /// </summary>
        /// <param name="naosOptions"></param>
        public static NaosServicesContextOptions AddServiceContext(
            this NaosServicesContextOptions naosOptions)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));
            EnsureArg.IsNotNull(naosOptions.Context, nameof(naosOptions.Context));

            if (naosOptions.Context.Descriptor.Product.IsNullOrEmpty())
            {
                throw new NaosException("SERVICE descriptor needs a productName");
            }

            if (naosOptions.Context.Descriptor.Capability.IsNullOrEmpty())
            {
                throw new NaosException("SERVICE descriptor needs a capabilityName");
            }

            naosOptions.Context.Services.AddTransient<HttpClientServiceContextHandler>();
            naosOptions.Context.Services.AddSingleton(sp =>
                new Naos.Foundation.ServiceDescriptor(
                    naosOptions.Context.Descriptor.Product,
                    naosOptions.Context.Descriptor.Capability,
                    naosOptions.Context.Descriptor.Version,
                    naosOptions.Context.Descriptor.Tags));

            naosOptions.Context.Services.AddHealthChecks()
                .AddCheck($"{naosOptions.Context.Descriptor.Name}-servicecontext", () => HealthCheckResult.Healthy(), tags: new[] { "live", "naos" });
            // TODO: add some strategy to control the healtyness of the service (for testing purposes)

            naosOptions.Context.Messages.Add("naos services builder: service context added");
            naosOptions.Context.Services.AddSingleton(new NaosFeatureInformation { Name = "ServiceContext", EchoRoute = "naos/servicecontext/echo" });

            return naosOptions;
        }
    }
}
