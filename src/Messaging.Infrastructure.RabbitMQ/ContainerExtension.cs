namespace Naos.Core.Messaging.Infrastructure.RabbitMQ
{
    using System.Collections.Generic;
    using System.Reflection;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using SimpleInjector;

    public static class ContainerExtension
    {
        public static Container AddNaosMessaging(
            this Container container,
            IConfiguration configuration,
            string section = "naos:messaging:rabbitMQ",
            IEnumerable<Assembly> assemblies = null)
        {
            var allAssemblies = new List<Assembly> { typeof(IMessageBus).GetTypeInfo().Assembly };
            if (!assemblies.IsNullOrEmpty())
            {
                allAssemblies.AddRange(assemblies);
            }

            var rabitMQConfiguration = configuration.GetSection(section).Get<RabbitMQConfiguration>();

            container.RegisterInstance(rabitMQConfiguration);
            container.Register(typeof(IMessageHandler<>), allAssemblies.DistinctBy(a => a.FullName)); // register all message handlers

            container.RegisterSingleton<IMessageBus>(() =>
                new RabbitMQMessageBus(
                        container.GetInstance<ILogger<RabbitMQMessageBus>>(),
                        rabitMQConfiguration,
                        new SimpleInjectorMessageHandlerFactory(container)));

            return container;
        }
    }
}
