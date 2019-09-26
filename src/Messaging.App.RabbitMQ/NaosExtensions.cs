namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using EnsureThat;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Messaging;
    using Naos.Messaging.App;
    using Naos.Messaging.Domain;
    using Naos.Messaging.Infrastructure.RabbitMQ;

    [ExcludeFromCodeCoverage]
    public static class NaosExtensions
    {
        public static MessagingOptions UseRabbitMQBroker(
            this MessagingOptions options,
            Action<IMessageBroker> brokerAction = null,
            string section = "naos:messaging:rabbitMQ",
            IEnumerable<Assembly> assemblies = null)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<IMessageBroker>(sp =>
            {
                var rabbitMQConfiguration = options.Context.Configuration.GetSection(section).Get<RabbitMQConfiguration>();
                var broker = new RabbitMQMessageBroker(o => o
                    .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                    .Host(rabbitMQConfiguration.Host)
                    .HandlerFactory(new ServiceProviderMessageHandlerFactory(sp)));

                brokerAction?.Invoke(broker);
                return broker;
            });

            options.Context.Messages.Add($"{LogKeys.Startup} naos services builder: messaging added (broker={nameof(RabbitMQMessageBroker)})");

            return options;
        }
    }
}
