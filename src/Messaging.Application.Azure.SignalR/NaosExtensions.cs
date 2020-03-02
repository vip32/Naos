namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Http;
    using EnsureThat;
    using Humanizer;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Infrastructure;
    using Naos.Messaging;
    using Naos.Messaging.Application;
    using Naos.Messaging.Domain;
    using Naos.Messaging.Infrastructure.Azure;
    using Naos.Tracing.Domain;

    [ExcludeFromCodeCoverage]
    public static class NaosExtensions
    {
        public static MessagingOptions UseSignalRServerlessBroker(
            this MessagingOptions options,
            Action<IMessageBroker> brokerAction = null,
            string messageScope = null,
            string section = "naos:messaging:signalr")
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            var configuration = options.Context.Configuration.GetSection(section).Get<SignalRConfiguration>();

            if (configuration?.Enabled == true)
            {
                options.Context.Services.AddScoped<IMessageBroker>(sp =>
                {
                    var broker = new SignalRServerlessMessageBroker(o => o
                            .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                            .Tracer(sp.GetService<ITracer>())
                            .Mediator((IMediator)sp.CreateScope().ServiceProvider.GetService(typeof(IMediator)))
                            .HandlerFactory(new ServiceProviderMessageHandlerFactory(sp))
                            .ConnectionString(configuration.ConnectionString)
                            .HttpClient(sp.GetRequiredService<IHttpClientFactory>())
                            .Subscriptions(sp.GetRequiredService<ISubscriptionMap>())
                            .FilterScope(Environment.GetEnvironmentVariable(EnvironmentKeys.IsLocal).ToBool()
                                ? Environment.MachineName.Humanize().Dehumanize().ToLower()
                                : string.Empty)
                            .MessageScope(messageScope));

                    brokerAction?.Invoke(broker);
                    return broker;
                });

                // TODO: does not work with azure hosted hub
                //options.Context.Services.AddHealthChecks()
                //    .AddSignalRHub(configuration.ConnectionString.SliceFrom("Endpoint=").SliceTill(";"), "messaging-broker-signalr", tags: new[] { "naos" });

                options.Context.Messages.Add($"{LogKeys.Startup} naos services builder: messaging added (broker={nameof(SignalRServerlessMessageBroker)})");
            }
            else
            {
                throw new NaosException("no messaging signalr is enabled");
            }

            return options;
        }
    }
}
