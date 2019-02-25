namespace Microsoft.Extensions.DependencyInjection
{
    using Naos.Core.Common.Web;
    using Naos.Core.RequestCorrelation.App.Web;
    using Naos.Core.ServiceContext.App.Web;

    public static class HttpClientBuilderExtensions
    {
        public static IHttpClientBuilder AddNaosHttpMessageHandlers(this IHttpClientBuilder builder)
        {
            return builder
                .AddHttpMessageHandler<HttpClientCorrelationHandler>()
                .AddHttpMessageHandler<HttpClientServiceContextHandler>()
                .AddHttpMessageHandler<HttpClientLogHandler>();
        }
    }
}
