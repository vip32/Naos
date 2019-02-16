namespace Microsoft.Extensions.DependencyInjection
{
    using EnsureThat;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Naos.Core.Common;
    using Naos.Core.RequestCorrelation.App;
    using Naos.Core.RequestCorrelation.App.Web;

    /// <summary>
    /// Extensions on the <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceExtensions
    {
        /// <summary>
        /// Adds required services to support the request correlation functionality.
        /// </summary>
        /// <param name="naosOptions"></param>
        /// <returns></returns>
        public static NaosOptions AddRequestCorrelation(
            this NaosOptions naosOptions)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));
            EnsureArg.IsNotNull(naosOptions.Context, nameof(naosOptions.Context));

            naosOptions.Context.Services.TryAddSingleton<ICorrelationContextAccessor, CorrelationContextAccessor>();
            naosOptions.Context.Services.TryAddTransient<ICorrelationContextFactory, CorrelationContextFactory>();
            naosOptions.Context.Services.AddTransient<HttpClientCorrelationHandler>();

            naosOptions.Context.Messages.Add($"{LogEventKeys.Startup} naos builder: request correlation added");

            return naosOptions;
        }
    }
}
