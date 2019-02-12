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
        /// <param name="context"></param>
        /// <returns></returns>
        public static INaosBuilderContext AddRequestCorrelation(
            this INaosBuilderContext context)
        {
            EnsureArg.IsNotNull(context, nameof(context));

            context.Services.TryAddSingleton<ICorrelationContextAccessor, CorrelationContextAccessor>();
            context.Services.TryAddTransient<ICorrelationContextFactory, CorrelationContextFactory>();
            context.Services.AddTransient<HttpClientCorrelationHandler>();

            context.Messages.Add($"{LogEventKeys.General} naos builder: request correlation added");

            return context;
        }
    }
}
