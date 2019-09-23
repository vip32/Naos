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
    using Naos.Messaging.App;
    using Naos.Messaging.Domain;
    using Naos.Messaging.Infrastructure.Azure;

    [ExcludeFromCodeCoverage]
    public static class NaosExtensions
    {
        public static MessagingOptions UseSignalRBroker(
            this MessagingOptions options,
            Action<IMessageBroker> brokerAction = null,
            string messageScope = null,
            string section = "naos:messaging:signalr")
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<IMessageBroker>(sp =>
            {
                var configuration = options.Context.Configuration.GetSection(section).Get<SignalRConfiguration>();
                var broker = new SignalRServerlessMessageBroker(o => o
                        .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                        .Mediator((IMediator)sp.CreateScope().ServiceProvider.GetService(typeof(IMediator)))
                        .HandlerFactory(new ServiceProviderMessageHandlerFactory(sp))
                        .ConnectionString(configuration.ConnectionString)
                        .HttpClient(sp.GetRequiredService<IHttpClientFactory>())
                        .Map(sp.GetRequiredService<ISubscriptionMap>())
                        .FilterScope(Environment.GetEnvironmentVariable(EnvironmentKeys.IsLocal).ToBool()
                            ? Environment.MachineName.Humanize().Dehumanize().ToLower()
                            : string.Empty)
                        .MessageScope(messageScope));

                brokerAction?.Invoke(broker);
                return broker;
            });

            options.Context.Messages.Add($"{LogKeys.Startup} naos services builder: messaging added (broker={nameof(SignalRServerlessMessageBroker)})");

            return options;
        }
    }
}
