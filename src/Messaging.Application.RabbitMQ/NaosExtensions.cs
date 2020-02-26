namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using EnsureThat;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Messaging;
    using Naos.Messaging.Application;
    using Naos.Messaging.Domain;
    using Naos.Messaging.Infrastructure;
    using Naos.Tracing.Domain;
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
            var configuration = options.Context.Configuration.GetSection(section).Get<RabbitMQConfiguration>() ?? new RabbitMQConfiguration();

            options.Context.Services.AddSingleton<IMessageBroker>(sp =>
            {
                var broker = new RabbitMQMessageBroker(o => o
                    .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                    .Tracer(sp.GetService<ITracer>())
                    .Mediator(sp.GetService<IMediator>())
                    .HandlerFactory(new ServiceProviderMessageHandlerFactory(sp))
                    //.MessageScope(options.Context.Descriptor.Name)
                    .ExchangeName(exchangeName)
                    .QueueName(queueName)
                    .Provider(sp.GetRequiredService<IRabbitMQProvider>()));

                brokerAction?.Invoke(broker);
                return broker;
            });

            options.Context.Services
                .AddSingleton<IConnectionFactory>(sp =>
                {
                    return new ConnectionFactory
                    {
                        Port = configuration.Port == 0 ? 5672 : configuration.Port,
                        HostName = configuration.Host.IsNullOrEmpty() ? "localhost" : configuration.Host, // or 'rabbitmq' in docker-compose env
                        UserName = configuration.UserName.IsNullOrEmpty() ? "guest" : configuration.UserName,
                        Password = configuration.Password.IsNullOrEmpty() ? "guest" : configuration.Password,
                        DispatchConsumersAsync = true
                    };
                })
                .AddSingleton<IRabbitMQProvider>(sp =>
                {
                    return new RabbitMQProvider(
                        sp.GetRequiredService<ILogger<RabbitMQProvider>>(),
                        sp.GetRequiredService<IConnectionFactory>(),
                        configuration.RetryCount);
                });

            options.Context.Services.AddHealthChecks()
                .AddRabbitMQ(sp => sp.GetRequiredService<IConnectionFactory>(), "messaging-broker-rabbitmq", tags: new[] { "naos"});

            options.Context.Messages.Add($"{LogKeys.Startup} naos services builder: messaging added (broker={nameof(RabbitMQMessageBroker)})");

            return options;
        }
    }
}
