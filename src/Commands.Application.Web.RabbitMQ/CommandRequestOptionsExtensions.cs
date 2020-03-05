namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Commands.Application.Web;
    using Naos.Foundation;
    using Naos.Foundation.Infrastructure;
    using Naos.Queueing.Domain;
    using Naos.Queueing.Infrastructure;
    using Naos.Tracing.Domain;
    using RabbitMQ.Client;

    [ExcludeFromCodeCoverage]
    public static class CommandRequestOptionsExtensions
    {
        public static CommandRequestOptions UseRabbitMQQueue(
            this CommandRequestOptions options,
            string name = "commandrequests",
            TimeSpan? expiration = null,
            int? retries = null)
        {
            var queueName = typeof(CommandRequestWrapper).PrettyName();
            var configuration = options.Context.Configuration.GetSection("naos:commands:rabbitMQQueue").Get<RabbitMQConfiguration>();

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

                options.Context.Services.AddScoped<IQueue<CommandRequestWrapper>>(sp =>
                {
                    var provider = new RabbitMQProvider(
                        sp.GetRequiredService<ILogger<RabbitMQProvider>>(),
                        connectionFactory,
                        configuration.RetryCount,
                        $"{LogKeys.Queueing} {queueName} ({sp.GetService<Naos.Foundation.ServiceDescriptor>()?.Name})");

                    return new RabbitMQQueue<CommandRequestWrapper>(o => o
                        .Mediator(sp.GetService<IMediator>())
                        .Tracer(sp.GetService<ITracer>())
                        .LoggerFactory(sp.GetService<ILoggerFactory>())
                        .Serializer(new JsonNetSerializer(TypedJsonSerializerSettings.Create())) // needs type information in json to deserialize correctly (which is needed for mediator.send)
                        .Provider(provider)
                        .QueueName($"{options.Context.Descriptor.Name}-{name}")
                        .Expiration(expiration)
                        .Retries(retries));
                });
            }

            return options;
        }
    }
}
