namespace Microsoft.Extensions.DependencyInjection
{
    using EnsureThat;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Naos.Core.Filtering.App;

    public static class ServiceRegistrations
    {
        public static IServiceCollection AddNaosFiltering(
            this IServiceCollection services)
        {
            EnsureArg.IsNotNull(services, nameof(services));

            services.TryAddSingleton<IFilterContextAccessor, FilterContextAccessor>();
            services.TryAddTransient<IFilterContextFactory, FilterContextFactory>();

            return services;
        }
    }
}
