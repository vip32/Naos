namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using EnsureThat;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Messaging;
    using Naos.Messaging.Application;
    using Naos.Messaging.Domain;
    using Naos.Messaging.Infrastructure.RabbitMQ;
    using RabbitMQ.Client;

    [ExcludeFromCodeCoverage]
    public static class NaosExtensions
    {
        public static MessagingOptions UseRabbitMQBroker(
            this MessagingOptions options,
            Action<IMessageBroker> brokerAction = null,
            string exchangeName = null,
            string queueName = null,
            string section = "naos:messaging:rabbitMQ",
            IEnumerable<Assembly> assemblies = null)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            queueName ??= options.Context.Descriptor.Name;
            var rabbitMQConfiguration = options.Context.Configuration.GetSection(section).Get<RabbitMQConfiguration>() ?? new RabbitMQConfiguration();

            options.Context.Services.AddSingleton<IMessageBroker>(sp =>
            {
                var broker = new RabbitMQMessageBroker(o => o
                    .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                    .HandlerFactory(new ServiceProviderMessageHandlerFactory(sp))
                    //.MessageScope(options.Context.Descriptor.Name)
                    .ExchangeName(exchangeName)
                    .QueueName(queueName)
                    .Provider(sp.GetRequiredService<IRabbitMQProvider>()));

                brokerAction?.Invoke(broker);
                return broker;
            });

            options.Context.Services.AddSingleton<IRabbitMQProvider>(sp =>
            {
                var factory = new ConnectionFactory()
                {
                    Port = rabbitMQConfiguration.Port == 0 ? 5672 : rabbitMQConfiguration.Port,
                    HostName = rabbitMQConfiguration.Host.IsNullOrEmpty() ? "localhost" : rabbitMQConfiguration.Host, // or 'rabbitmq' in docker-compose env
                    UserName = rabbitMQConfiguration.UserName.IsNullOrEmpty() ? "guest" : rabbitMQConfiguration.UserName,
                    Password = rabbitMQConfiguration.Password.IsNullOrEmpty() ? "guest" : rabbitMQConfiguration.Password,
                    DispatchConsumersAsync = true
                };

                return new RabbitMQProvider(
                    sp.GetRequiredService<ILogger<RabbitMQProvider>>(),
                    factory,
                    rabbitMQConfiguration.RetryCount);
            });

            //options.Context.Services.AddHealthChecks()
            //    .AddAzureServiceBusTopic(configuration.ConnectionString, configuration.EntityPath, "messaging-broker-servicebus");

            options.Context.Messages.Add($"{LogKeys.Startup} naos services builder: messaging added (broker={nameof(RabbitMQMessageBroker)})");

            return options;
        }
    }
}
