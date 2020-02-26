namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Net.Http;
    using Humanizer;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Application;
    using Naos.RequestCorrelation.Application.Web;
    using Naos.ServiceContext.Application.Web;
    using Naos.Tracing.Application;
    using Polly;
    using Polly.Extensions.Http;

    public static class HttpClientBuilderExtensions
    {
        public static IHttpClientBuilder AddNaosMessageHandlers(this IHttpClientBuilder builder, Action<IHttpClientBuilder> setupAction = null)
        {
            if (setupAction != null)
            {
                setupAction.Invoke(builder);
                return builder;
            }
            else
            {
                // default handlers
                return builder
                    .AddHttpMessageHandler<HttpClientCorrelationHandler>()
                    .AddHttpMessageHandler<HttpClientServiceContextHandler>()
                    .AddHttpMessageHandler<HttpClientTracerHandler>()
                    .AddHttpMessageHandler<HttpClientLogHandler>();
            }
        }

        public static IHttpClientBuilder AddNaosPolicyHandlers(this IHttpClientBuilder builder, Action<IHttpClientBuilder> setupAction = null)
        {
            if (setupAction != null)
            {
                setupAction.Invoke(builder);
                return builder;
            }
            else
            {
                // default policies
                return builder
                   .AddPolicyHandler((sp, req) =>
                        HttpPolicyExtensions.HandleTransientHttpError()
                            .WaitAndRetryAsync(
                                3,
                                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                                onRetry: (outcome, timespan, retryAttempt, context) =>
                                {
                                    sp.GetService<ILogger<HttpClient>>()
                                        .LogWarning($"{{LogKey:l}} delaying for {timespan.Humanize()}, then making retry #{retryAttempt}: {outcome.Exception.GetFullMessage()}", LogKeys.OutboundRequest);
                                }))
                    .AddPolicyHandler((sp, req) =>
                        HttpPolicyExtensions.HandleTransientHttpError()
                            .CircuitBreakerAsync(
                                3,
                                durationOfBreak: TimeSpan.FromSeconds(30),
                                onBreak: (response, state) =>
                                {
                                    sp.GetService<ILogger<HttpClient>>().LogWarning($"{{LogKey:l}} break circuit ({state.Humanize()}): {response.Exception.GetFullMessage()}", LogKeys.OutboundRequest);
                                },
                                onReset: () =>
                                {
                                    sp.GetService<ILogger<HttpClient>>().LogInformation("{LogKey:l} reset circuit", LogKeys.OutboundRequest);
                                }));
            }
        }
    }
}
