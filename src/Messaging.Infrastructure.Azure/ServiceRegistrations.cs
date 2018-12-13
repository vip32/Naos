namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using Humanizer;
    using Microsoft.Azure.Management.ResourceManager.Fluent;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Infrastructure.Azure.ServiceBus;
    using Naos.Core.Messaging;
    using Naos.Core.Messaging.Infrastructure.Azure.ServiceBus;

    public static class ServiceRegistrations
    {
        public static IServiceCollection AddNaosMessaging(
            this IServiceCollection services,
            IConfiguration configuration,
            string topicName = null,
            string subscriptionName = null,
            string section = "naos:messaging:serviceBus")
        {
            services.Scan(scan => scan // https://andrewlock.net/using-scrutor-to-automatically-register-your-services-with-the-asp-net-core-di-container/
                .FromExecutingAssembly()
                .FromApplicationDependencies(a => !a.FullName.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase) && !a.FullName.StartsWith("System", StringComparison.OrdinalIgnoreCase))
                .AddClasses(classes => classes.AssignableTo(typeof(IMessageHandler<>)), true));

            services.AddScoped<IServiceBusProvider>(sp =>
            {
                var serviceBusConfiguration = configuration.GetSection(section).Get<ServiceBusConfiguration>();
                serviceBusConfiguration.EntityPath = topicName ?? $"{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}-Naos.Messaging";

                if (serviceBusConfiguration?.Enabled == true)
                {
                    return new ServiceBusProvider(
                        sp.GetRequiredService<ILogger<ServiceBusProvider>>(),
                        SdkContext.AzureCredentialsFactory.FromServicePrincipal(serviceBusConfiguration.ClientId, serviceBusConfiguration.ClientSecret, serviceBusConfiguration.TenantId, AzureEnvironment.AzureGlobalCloud),
                        serviceBusConfiguration);
                }

                throw new NotImplementedException("no messaging servicebus is enabled");
            });

            services.AddSingleton<IMessageBroker>(sp =>
                new ServiceBusMessageBroker(
                        sp.GetRequiredService<ILogger<ServiceBusMessageBroker>>(),
                        sp.GetRequiredService<IServiceBusProvider>(),
                        new ServiceProviderMessageHandlerFactory(sp),
                        subscriptionName: subscriptionName ?? AppDomain.CurrentDomain.FriendlyName, // PRODUCT.CAPABILITY
                        filterScope: Environment.GetEnvironmentVariable("ASPNETCORE_ISLOCAL").ToBool()
                            ? Environment.MachineName.Humanize().Dehumanize().ToLower()
                            : string.Empty)); // scope the messagebus messages to the local machine, so local events are handled locally

            return services;
        }
    }
}
