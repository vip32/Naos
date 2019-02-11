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

    public static class ServiceExtensions
    {
        public static ServiceConfigurationContext AddMessagingRabbitMQ(
            this ServiceConfigurationContext context,
            Action<IMessageBroker> setupAction = null,
            string section = "naos:messaging:rabbitMQ",
            IEnumerable<Assembly> assemblies = null)
        {
            EnsureArg.IsNotNull(context, nameof(context));

            context.Services.Scan(scan => scan // https://andrewlock.net/using-scrutor-to-automatically-register-your-services-with-the-asp-net-core-di-container/
               .FromExecutingAssembly()
               .FromApplicationDependencies(a => !a.FullName.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase) && !a.FullName.StartsWith("System", StringComparison.OrdinalIgnoreCase))
               .AddClasses(classes => classes.AssignableTo(typeof(IMessageHandler<>)), true));

            context.Services.AddSingleton<IMessageBroker>(sp =>
            {
                var result = new RabbitMQMessageBroker(o => o
                    .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                    .Configuration(context.Configuration.GetSection(section).Get<RabbitMQConfiguration>())
                    .HandlerFactory(new ServiceProviderMessageHandlerFactory(sp)));

                setupAction?.Invoke(result);
                return result;
            });

            return context;
        }
    }
}
