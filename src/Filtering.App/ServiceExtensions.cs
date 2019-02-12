namespace Microsoft.Extensions.DependencyInjection
{
    using EnsureThat;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Naos.Core.RequestFiltering.App;
    using Naos.Core.RequestFiltering.App.Web;

    public static class ServiceExtensions
    {
        /// <summary>
        /// Adds required services to support the request filtering functionality.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static INaosBuilderContext AddRequestFiltering(
            this INaosBuilderContext context)
        {
            EnsureArg.IsNotNull(context, nameof(context));

            context.Services.TryAddSingleton<IFilterContextAccessor, FilterContextAccessor>();
            context.Services.TryAddTransient<IFilterContextFactory, FilterContextFactory>();

            return context;
        }
    }
}
