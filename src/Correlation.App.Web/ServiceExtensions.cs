namespace Naos.Core.Correlation.App.Web
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Naos.Core.Correlation.Domain;

    /// <summary>
    /// Extensions on the <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceExtensions
    {
        /// <summary>
        /// Adds required services to support the Correlation ID functionality.
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <returns></returns>
        public static IServiceCollection AddNaosCorrelation(this IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddSingleton<ICorrelationContextAccessor, CorrelationContextAccessor>();
            serviceCollection.TryAddTransient<ICorrelationContextFactory, CorrelationContextFactory>();

            return serviceCollection;
        }
    }
}
