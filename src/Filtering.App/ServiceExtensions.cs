namespace Microsoft.Extensions.DependencyInjection
{
    using EnsureThat;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Naos.Core.RequestFiltering.App;

    public static class ServiceExtensions
    {
        public static ServiceConfigurationContext AddRequestFiltering(
            this ServiceConfigurationContext context)
        {
            EnsureArg.IsNotNull(context, nameof(context));

            context.Services.TryAddSingleton<IFilterContextAccessor, FilterContextAccessor>();
            context.Services.TryAddTransient<IFilterContextFactory, FilterContextFactory>();

            return context;
        }
    }
}
