namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using Naos.Core.ServiceDiscovery.App;

    public static class ServicesExtensions
    {
        public static IHttpClientBuilder AddNaosServiceClient<TClient>(this IServiceCollection services, Action<IHttpClientBuilder> setupAction = null)
            where TClient : ServiceDiscoveryClient
        {
            if (setupAction != null)
            {
                var builder = services
                    .AddHttpClient<TClient>();
                setupAction.Invoke(builder);
                return builder;
            }
            else
            {
                return services
                    .AddHttpClient<TClient>()
                    .AddNaosMessageHandlers()
                    .AddNaosPolicyHandlers();
            }
        }
    }
}
