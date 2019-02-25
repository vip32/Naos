namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using EnsureThat;
    using Naos.Core.ServiceDiscovery.App;

    public static class NaosExtensions
    {
        public static NaosOptions AddServiceClient<TClient>(this NaosOptions naosOptions, Action<IHttpClientBuilder> setupAction = null)
            where TClient : ServiceDiscoveryClient
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));
            EnsureArg.IsNotNull(naosOptions.Context, nameof(naosOptions.Context));

            naosOptions.Context.Services.AddNaosServiceClient<TClient>(setupAction);

            return naosOptions;
        }

        public static NaosOptions AddServiceClient(this NaosOptions naosOptions, string name, Action<IHttpClientBuilder> setupAction = null)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));
            EnsureArg.IsNotNull(naosOptions.Context, nameof(naosOptions.Context));
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));

            naosOptions.Context.Services.AddNaosServiceClient(name, setupAction);

            return naosOptions;
        }
    }
}
