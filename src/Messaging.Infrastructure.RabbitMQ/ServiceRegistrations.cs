namespace Naos.Core.Messaging.Infrastructure.RabbitMQ
{
    using System;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Base app service collection registrations
    /// </summary>
    public static class ServiceRegistrations
    {
        public static IServiceCollection AddNaosMessaging(this IServiceCollection services, IConfiguration configuration, string topicName = null, string subscriptionName = null)
        {
            return services
                .AddOptions()
                .Configure<RabbitMQConfiguration>(configuration.GetSection("naos:messaging:rabbitMQ"))
                .AddSingleton<IMessageBus, RabbitMQMessageBus>(sp =>
                {
                    var rabbitMQConfiguration = sp.GetService<IOptions<RabbitMQConfiguration>>();
                    if (rabbitMQConfiguration?.Value?.Enabled == true)
                    {
                        return new RabbitMQMessageBus(rabbitMQConfiguration.Value);
                    }

                    throw new NotImplementedException("no message bus implementation is registered");
                });
        }
    }
}
