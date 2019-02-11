namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Net.Http;
    using EnsureThat;
    using Humanizer;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Infrastructure.Azure;
    using Naos.Core.Messaging;
    using Naos.Core.Messaging.App.Web;
    using Naos.Core.Messaging.Infrastructure.Azure;

    public static class ServiceExtensions
    {
        public static ServiceConfigurationContext AddMessagingSignalR(
            this ServiceConfigurationContext context,
            Action<IMessageBroker> setupAction = null,
            string messageScope = null,
            string section = "naos:messaging:signalr")
        {
            EnsureArg.IsNotNull(context, nameof(context));

            context.Services.Scan(scan => scan // https://andrewlock.net/using-scrutor-to-automatically-register-your-services-with-the-asp-net-core-di-container/
                .FromExecutingAssembly()
                .FromApplicationDependencies(a => !a.FullName.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase) && !a.FullName.StartsWith("System", StringComparison.OrdinalIgnoreCase))
                .AddClasses(classes => classes.AssignableTo(typeof(IMessageHandler<>)), true));

            context.Services.AddSingleton<Hosting.IHostedService>(sp =>
                    new MessagingHostedService(sp.GetRequiredService<ILogger<MessagingHostedService>>(), sp));

            context.Services.AddSingleton<ISubscriptionMap, SubscriptionMap>();
            context.Services.AddSingleton<IMessageBroker>(sp =>
            {
                var signalRConfiguration = context.Configuration.GetSection(section).Get<SignalRConfiguration>();
                var result = new SignalRServerlessMessageBroker(o => o
                        .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                        .Mediator((IMediator)sp.CreateScope().ServiceProvider.GetService(typeof(IMediator)))
                        .HandlerFactory(new ServiceProviderMessageHandlerFactory(sp))
                        .Configuration(signalRConfiguration)
                        .HttpClient(sp.GetRequiredService<IHttpClientFactory>())
                        .Map(sp.GetRequiredService<ISubscriptionMap>())
                        .FilterScope(Environment.GetEnvironmentVariable(EnvironmentKeys.IsLocal).ToBool()
                            ? Environment.MachineName.Humanize().Dehumanize().ToLower()
                            : string.Empty)
                        .MessageScope(messageScope));

                setupAction?.Invoke(result);
                return result;
            });

            return context;
        }
    }
}
