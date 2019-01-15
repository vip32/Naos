namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Linq;
    using EnsureThat;
    using Microsoft.Extensions.Configuration;
    using Naos.Core.Common;
    using Naos.Core.ServiceContext.App;
    using Naos.Core.ServiceContext.App.Web;

    /// <summary>
    /// Extensions on the <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceExtensions
    {
        /// <summary>
        /// Adds required services to support the Discovery functionality.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="productName"></param>
        /// <param name="capabilityName"></param>
        /// <param name="version"></param>
        /// <param name="tags"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public static IServiceCollection AddNaosServiceContext(
            this IServiceCollection services,
            IConfiguration configuration,
            string productName = null,
            string capabilityName = null,
            string version = null,
            string[] tags = null,
            string section = "naos:serviceContext")
        {
            EnsureArg.IsNotNull(services, nameof(services));

            var serviceContextConfiguration = configuration.GetSection(section).Get<ServiceContextConfiguration>();
            productName = productName ?? serviceContextConfiguration?.ProductName;
            capabilityName = capabilityName ?? serviceContextConfiguration?.CapabilityName;

            if (productName.IsNullOrEmpty())
            {
                throw new NaosException("SERVICE descriptor needs a productName");
            }

            if (capabilityName.IsNullOrEmpty())
            {
                throw new NaosException("SERVICE descriptor needs a capabilityName");
            }

            version = version ?? serviceContextConfiguration?.Version ?? "1.0.0";
            tags = tags ?? serviceContextConfiguration?.Tags;

            if (!tags.Any())
            {
                tags = new[] { capabilityName }; // tags are used for service discovery too
            }

            services.AddTransient<HttpClientServiceContextHandler>();
            services.AddSingleton(sp => new Naos.Core.App.ServiceDescriptor
            {
                Product = productName ?? AppDomain.CurrentDomain.FriendlyName.SubstringTillLast("."),
                Capability = capabilityName ?? AppDomain.CurrentDomain.FriendlyName.SubstringFromLast("."),
                Version = version, // read from fileversion?
                Tags = tags
            });

            return services;
        }
    }
}
