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
    using Naos.Core.Common;
    using Naos.Core.Infrastructure.Azure;
    using Naos.Core.Messaging;
    using Naos.Core.Messaging.App;
    using Naos.Core.Messaging.Infrastructure.Azure;

    [ExcludeFromCodeCoverage]
    public static class NaosExtensions
    {
        public static MessagingOptions UseSignalRBroker(
            this MessagingOptions options,
            Action<IMessageBroker> setupAction = null,
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
                        .Configuration(configuration)
                        .HttpClient(sp.GetRequiredService<IHttpClientFactory>())
                        .Map(sp.GetRequiredService<ISubscriptionMap>())
                        .FilterScope(Environment.GetEnvironmentVariable(EnvironmentKeys.IsLocal).ToBool()
                            ? Environment.MachineName.Humanize().Dehumanize().ToLower()
                            : string.Empty)
                        .MessageScope(messageScope));

                setupAction?.Invoke(broker);
                return broker;
            });

            options.Context.Messages.Add($"{LogEventKeys.Startup} naos services builder: messaging added (broker={nameof(SignalRServerlessMessageBroker)})");

            return options;
        }
    }
}
