namespace Naos.Core.Messaging.Infrastructure.RabbitMQ
{
    using System.Collections.Generic;
    using System.Reflection;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using SimpleInjector;

    public static class ContainerExtensions
    {
        public static Container AddNaosMessaging(
            this Container container,
            IConfiguration configuration,
            string section = "naos:messaging:rabbitMQ",
            IEnumerable<Assembly> assemblies = null)
        {
            var allAssemblies = new List<Assembly> { typeof(IMessageBroker).GetTypeInfo().Assembly };
            if (!assemblies.IsNullOrEmpty())
            {
                allAssemblies.AddRange(assemblies);
            }

            container.Register(typeof(IMessageHandler<>), allAssemblies.DistinctBy(a => a.FullName)); // register all message handlers
            container.RegisterSingleton<IMessageBroker>(() =>
                new RabbitMQMessageBroker(
                        container.GetInstance<ILogger<RabbitMQMessageBroker>>(),
                        configuration.GetSection(section).Get<RabbitMQConfiguration>(),
                        new SimpleInjectorMessageHandlerFactory(container)));

            return container;
        }
    }
}
