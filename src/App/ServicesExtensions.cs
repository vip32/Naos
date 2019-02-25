namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using EnsureThat;
    using Naos.Core.ServiceDiscovery.App;

    public static class ServicesExtensions
    {
        public static IServiceCollection AddNaosServiceClient<TClient>(this IServiceCollection services, Action<IHttpClientBuilder> setupAction = null)
            where TClient : ServiceDiscoveryClient
        {
            EnsureArg.IsNotNull(services, nameof(services));

            if (setupAction != null)
            {
                var builder = services
                    .AddHttpClient<TClient>();
                setupAction.Invoke(builder);
            }
            else
            {
                services
                    .AddHttpClient<TClient>()
                    .AddNaosMessageHandlers()
                    .AddNaosPolicyHandlers();
            }

            return services;
        }

        public static IServiceCollection AddNaosServiceClient(this IServiceCollection services, string name, Action<IHttpClientBuilder> setupAction = null)
        {
            EnsureArg.IsNotNull(services, nameof(services));
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));

            if (setupAction != null)
            {
                var builder = services
                    .AddHttpClient(name);
                setupAction.Invoke(builder);
            }
            else
            {
                services
                    .AddHttpClient(name)
                    .AddNaosMessageHandlers()
                    .AddNaosPolicyHandlers();
            }

            return services;
        }
    }
}
