namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using EnsureThat;
    using MediatR;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Infrastructure;
    using Naos.Queueing.Application;
    using Naos.Queueing.Domain;
    using Naos.Queueing.Infrastructure;
    using Naos.Tracing.Domain;
    using RabbitMQ.Client;

    [ExcludeFromCodeCoverage]
    public static class NaosExtensions
    {
        public static QueueingOptions UseRabbitMQQueue<TData>(
            this QueueingOptions options,
            Action<QueueingProviderOptions<TData>> optionsAction = null,
            string section = "naos:queueing:serviceBus")
            where TData : class
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            var configuration = options.Context.Configuration.GetSection(section).Get<RabbitMQConfiguration>();
            options.Context.Services.AddSingleton<IQueue<TData>>(sp =>
            {
                if (configuration?.Enabled == true)
                {
                    var connectionFactory = new ConnectionFactory
                    {
                        Port = configuration.Port == 0 ? 5672 : configuration.Port,
                        HostName = configuration.Host.IsNullOrEmpty() ? "localhost" : configuration.Host, // or 'rabbitmq' in docker-compose env
                        UserName = configuration.UserName.IsNullOrEmpty() ? "guest" : configuration.UserName,
                        Password = configuration.Password.IsNullOrEmpty() ? "guest" : configuration.Password,
                        DispatchConsumersAsync = true
                    };

                    var provider = new RabbitMQProvider(
                        sp.GetRequiredService<ILogger<RabbitMQProvider>>(),
                        sp.GetRequiredService<IConnectionFactory>(),
                        configuration.RetryCount);

                    return new RabbitMQQueue<TData>(o => o
                        .Mediator(sp.GetService<IMediator>())
                        .Tracer(sp.GetService<ITracer>())
                        .LoggerFactory(sp.GetService<ILoggerFactory>())
                        .Provider(provider)
                        .NoRetries());
                }

                throw new NotImplementedException("no queueing rabbitmq is enabled");
            });

            //optionsAction?.Invoke(broker);

            //options.Context.Services.AddHealthChecks()
            //    .AddAzureServiceBusTopic(configuration.ConnectionString, configuration.EntityPath, "messaging-broker-servicebus");

            options.Context.Messages.Add($"{LogKeys.Startup} naos services builder: queueing added (provider={nameof(RabbitMQQueue<TData>)})");

            return options;
        }
    }
}
