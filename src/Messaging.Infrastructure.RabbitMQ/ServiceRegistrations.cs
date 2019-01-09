namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using EnsureThat;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Messaging;
    using Naos.Core.Messaging.Infrastructure.RabbitMQ;

    public static class ServiceRegistrations
    {
        public static IServiceCollection AddNaosMessagingRabbitMQ(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<IMessageBroker> setupAction = null,
            string section = "naos:messaging:rabbitMQ",
            IEnumerable<Assembly> assemblies = null)
        {
            EnsureArg.IsNotNull(services, nameof(services));

            services.Scan(scan => scan // https://andrewlock.net/using-scrutor-to-automatically-register-your-services-with-the-asp-net-core-di-container/
               .FromExecutingAssembly()
               .FromApplicationDependencies(a => !a.FullName.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase) && !a.FullName.StartsWith("System", StringComparison.OrdinalIgnoreCase))
               .AddClasses(classes => classes.AssignableTo(typeof(IMessageHandler<>)), true));

            services.AddSingleton<IMessageBroker>(sp =>
            {
                var result = new RabbitMQMessageBroker(
                        sp.GetRequiredService<ILogger<RabbitMQMessageBroker>>(),
                        configuration.GetSection(section).Get<RabbitMQConfiguration>(),
                        new ServiceProviderMessageHandlerFactory(sp));

                setupAction?.Invoke(result);
                return result;
            });

            return services;
        }
    }
}
