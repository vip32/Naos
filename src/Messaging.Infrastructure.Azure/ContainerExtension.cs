namespace Naos.Core.Messaging.Infrastructure.Azure
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Humanizer;
    using Microsoft.Azure.Management.ResourceManager.Fluent;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Infrastructure.ServiceBus;
    using Naos.Core.Messaging.Infrastructure.Azure.ServiceBus;
    using SimpleInjector;

    public static class ContainerExtension
    {
        public static Container AddNaosMessaging(
            this Container container,
            IConfiguration configuration,
            string topicName = null,
            string subscriptionName = null,
            string section = "naos:messaging:serviceBus",
            IEnumerable<Assembly> assemblies = null)
        {
            var allAssemblies = new List<Assembly> { typeof(IMessageBus).GetTypeInfo().Assembly };
            if (!assemblies.IsNullOrEmpty())
            {
                allAssemblies.AddRange(assemblies);
            }

            container.Register(typeof(IMessageHandler<>), allAssemblies.DistinctBy(a => a.FullName)); // register all message handlers
            container.RegisterSingleton<IServiceBusProvider>(() =>
            {
                var serviceBusConfiguration = configuration.GetSection(section).Get<ServiceBusConfiguration>();
                serviceBusConfiguration.EntityPath = topicName ?? $"{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}-Naos.Messaging";

                if (serviceBusConfiguration?.Enabled == true)
                {
                    return new ServiceBusProvider(
                        container.GetInstance<ILogger<ServiceBusProvider>>(),
                        SdkContext.AzureCredentialsFactory.FromServicePrincipal(serviceBusConfiguration.ClientId, serviceBusConfiguration.ClientSecret, serviceBusConfiguration.TenantId, AzureEnvironment.AzureGlobalCloud),
                        serviceBusConfiguration);
                }

                throw new NotImplementedException("no messaging servicebus is enabled");
            });
            container.RegisterSingleton<IMessageBus>(() =>
                new ServiceBusMessageBus(
                        container.GetInstance<ILogger<ServiceBusMessageBus>>(),
                        container.GetInstance<IServiceBusProvider>(),
                        new SimpleInjectorMessageHandlerFactory(container),
                        subscriptionName: subscriptionName ?? AppDomain.CurrentDomain.FriendlyName, // PRODUCT.CAPABILITY
                        filterScope: Environment.GetEnvironmentVariable("ASPNETCORE_ISLOCAL").ToBool()
                            ? Environment.MachineName.Humanize().Dehumanize().ToLower()
                            : string.Empty)); // scope the messagebus messages to the local machine, so local events are handled locally

            return container;
        }
    }
}
