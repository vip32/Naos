namespace Microsoft.Extensions.DependencyInjection
{
    using EnsureThat;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Naos.Core.Common;
    using Naos.Core.RequestFiltering.App;
    using Naos.Core.RequestFiltering.App.Web;

    public static class ServiceExtensions
    {
        /// <summary>
        /// Adds required services to support the request filtering functionality.
        /// </summary>
        /// <param name="naosOptions"></param>
        /// <returns></returns>
        public static NaosOptions AddRequestFiltering(
            this NaosOptions naosOptions)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));
            EnsureArg.IsNotNull(naosOptions.Context, nameof(naosOptions.Context));

            naosOptions.Context.Services.TryAddSingleton<IFilterContextAccessor, FilterContextAccessor>();
            naosOptions.Context.Services.TryAddTransient<IFilterContextFactory, FilterContextFactory>();

            naosOptions.Context.Messages.Add($"{LogEventKeys.General} naos builder: request filtering added");

            return naosOptions;
        }
    }
}
