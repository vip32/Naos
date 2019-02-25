namespace Microsoft.Extensions.DependencyInjection
{
    using Naos.Core.ServiceDiscovery.App;

    public static class ServicesExtensions
    {
        public static IServiceCollection AddNaosServiceClient<TClient>(this IServiceCollection services)
            where TClient : ServiceDiscoveryClient
        {
            services
                .AddHttpClient<TClient>()
                // TODO: accept some policies
                .AddNaosHttpMessageHandlers();

            return services;
        }
    }
}
