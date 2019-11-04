namespace Microsoft.Extensions.DependencyInjection
{
    using System.Diagnostics.CodeAnalysis;
    using EnsureThat;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;
    using Naos.Configuration.Application;
    using Naos.RequestFiltering.Application;
    using Naos.RequestFiltering.Application.Web;

    [ExcludeFromCodeCoverage]
    public static class NaosExtensions
    {
        /// <summary>
        /// Adds required services to support the request filtering functionality.
        /// </summary>
        /// <param name="naosOptions"></param>
        public static NaosServicesContextOptions AddRequestFiltering(
            this NaosServicesContextOptions naosOptions)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));
            EnsureArg.IsNotNull(naosOptions.Context, nameof(naosOptions.Context));

            naosOptions.Context.Services.TryAddSingleton<IFilterContextAccessor, FilterContextAccessor>();
            naosOptions.Context.Services.TryAddTransient<IFilterContextFactory, FilterContextFactory>();

            naosOptions.Context.Messages.Add($"{LogKeys.Startup} naos services builder: request filtering added");
            naosOptions.Context.Services.AddSingleton(new NaosFeatureInformation { Name = "RequestFiltering", EchoRoute = "api/echo/requestfiltering?q=name=eq:naos,epoch=lt:12345&order=name" });

            return naosOptions;
        }
    }
}
