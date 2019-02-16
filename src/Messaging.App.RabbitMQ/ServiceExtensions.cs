namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using EnsureThat;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Messaging;
    using Naos.Core.Messaging.App;
    using Naos.Core.Messaging.Infrastructure.RabbitMQ;

    public static class ServiceExtensions
    {
        public static MessagingOptions UseRabbitMQBroker(
            this MessagingOptions options,
            Action<IMessageBroker> setupAction = null,
            string section = "naos:messaging:rabbitMQ",
            IEnumerable<Assembly> assemblies = null)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<IMessageBroker>(sp =>
            {
                var broker = new RabbitMQMessageBroker(o => o
                    .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                    .Configuration(options.Context.Configuration.GetSection(section).Get<RabbitMQConfiguration>())
                    .HandlerFactory(new ServiceProviderMessageHandlerFactory(sp)));

                setupAction?.Invoke(broker);
                return broker;
            });

            options.Context.Messages.Add($"{LogEventKeys.Startup} naos builder: messaging added (broker={nameof(RabbitMQMessageBroker)})");

            return options;
        }
    }
}
