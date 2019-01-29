namespace Microsoft.Extensions.DependencyInjection
{
    using EnsureThat;
    using Naos.Core.Common;
    using Naos.Core.ServiceContext.App.Web;

    /// <summary>
    /// Extensions on the <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceExtensions
    {
        /// <summary>
        /// Adds required services to support the service context functionality.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static ServiceConfigurationContext AddServiceContext(
            this ServiceConfigurationContext context)
        {
            EnsureArg.IsNotNull(context, nameof(context));

            if (context.Descriptor.Product.IsNullOrEmpty())
            {
                throw new NaosException("SERVICE descriptor needs a productName");
            }

            if (context.Descriptor.Capability.IsNullOrEmpty())
            {
                throw new NaosException("SERVICE descriptor needs a capabilityName");
            }

            context.Services.AddTransient<HttpClientServiceContextHandler>();
            context.Services.AddSingleton(sp => new Naos.Core.Commands.ServiceDescriptor
            {
                Product = context.Descriptor.Product, // ?? AppDomain.CurrentDomain.FriendlyName.SubstringTillLast("."),
                Capability = context.Descriptor.Capability, // ?? AppDomain.CurrentDomain.FriendlyName.SubstringFromLast("."),
                Version = context.Descriptor.Version, // read from fileversion?
                Tags = context.Descriptor.Tags
            });

            return context;
        }
    }
}
