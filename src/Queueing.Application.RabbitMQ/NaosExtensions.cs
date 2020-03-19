namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using EnsureThat;
    using MediatR;
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
            TimeSpan? expiration = null,
            int? retries = null,
            string section = "naos:queueing:rabbitMQ")
            where TData : class
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            var queueName = typeof(TData).PrettyName();
            var configuration = options.Context.Configuration.GetSection(section).Get<RabbitMQConfiguration>();

            if (configuration?.Enabled == true)
            {
                var connectionFactory = new ConnectionFactory
                {
                    Port = configuration.Port == 0 ? 5672 : configuration.Port,
                    HostName = configuration.Host.IsNullOrEmpty() ? "localhost" : configuration.Host, // or 'rabbitmq' in docker-compose env
                    UserName = configuration.UserName.IsNullOrEmpty() ? "guest" : configuration.UserName,
                    Password = configuration.Password.IsNullOrEmpty() ? "guest" : configuration.Password,
                    DispatchConsumersAsync = true,
                };

                options.Context.Services.AddScoped<IQueue<TData>>(sp =>
                {
                    var provider = new RabbitMQProvider(
                        sp.GetRequiredService<ILogger<RabbitMQProvider>>(),
                        connectionFactory,
                        configuration.RetryCount,
                        $"{LogKeys.Queueing} {queueName} ({sp.GetService<Naos.Foundation.ServiceDescriptor>()?.Name})");

                    return new RabbitMQQueue<TData>(o => o
                        .Mediator(sp.GetService<IMediator>())
                        .Tracer(sp.GetService<ITracer>())
                        .LoggerFactory(sp.GetService<ILoggerFactory>())
                        .Provider(provider)
                        .QueueName(queueName)
                        .Expiration(expiration)
                        .Retries(retries));
                });

                optionsAction?.Invoke(
                    new QueueingProviderOptions<TData>(options.Context));

                options.Context.Services.AddHealthChecks()
                    .AddRabbitMQ(sp => connectionFactory, $"queueing-provider-rabbitmq-{queueName}", tags: new[] { "naos" });

                options.Context.Messages.Add($"naos services builder: queueing provider added (provider={nameof(RabbitMQQueue<TData>)}, queue={queueName})");
            }
            else
            {
                throw new NaosException("no queueing rabbitmq is enabled");
            }

            return options;
        }
    }
}
