namespace Microsoft.Extensions.DependencyInjection
{
    using EnsureThat;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Naos.Core.RequestCorrelation.App;
    using Naos.Core.RequestCorrelation.App.Web;

    /// <summary>
    /// Extensions on the <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceExtensions
    {
        /// <summary>
        /// Adds required services to support the Correlation ID functionality.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddNaosRequestCorrelation(this IServiceCollection services)
        {
            EnsureArg.IsNotNull(services, nameof(services));

            services.TryAddSingleton<ICorrelationContextAccessor, CorrelationContextAccessor>();
            services.TryAddTransient<ICorrelationContextFactory, CorrelationContextFactory>();
            services.AddTransient<HttpClientCorrelationHandler>();

            return services;
        }
    }
}
