namespace Naos.Core.Messaging.Infrastructure.Azure.ServiceBus
{
    using System;
    using Humanizer;
    using Microsoft.Azure.Management.ResourceManager.Fluent;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Naos.Core.Common;
    using Naos.Core.Infrastructure.ServiceBus;

    /// <summary>
    /// Base app service collection registrations
    /// </summary>
    public static class ServiceRegistrations
    {
        public static IServiceCollection AddNaosMessaging(this IServiceCollection services, IConfiguration configuration, string topicName = null, string subscriptionName = null, string section = "naos:messaging:serviceBus")
        {
            return services
                .AddOptions()
                .Configure<ServiceBusConfiguration>(configuration.GetSection(section))
                .AddSingleton<IServiceBusProvider>(sp =>
                {
                    var serviceBusConfiguration = sp.GetService<IOptions<ServiceBusConfiguration>>();
                    serviceBusConfiguration.Value.EntityPath = topicName ?? $"{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}-Naos.Messaging";

                    if (serviceBusConfiguration?.Value?.Enabled == true)
                    {
                        return new ServiceBusProvider(
                            sp.GetService<ILogger<ServiceBusProvider>>(),
                            SdkContext.AzureCredentialsFactory.FromServicePrincipal(serviceBusConfiguration.Value.ClientId, serviceBusConfiguration.Value.ClientSecret, serviceBusConfiguration.Value.TenantId, AzureEnvironment.AzureGlobalCloud),
                            serviceBusConfiguration);
                    }

                    // TODO: otherwise register rabittmq
                    throw new NotImplementedException("no message bus implementation is registered");
                })
                .AddSingleton<IMessageBus, ServiceBusMessageBus>(sp =>
                {
                    return new ServiceBusMessageBus(
                        sp.GetRequiredService<ILogger<ServiceBusMessageBus>>(),
                        sp.GetRequiredService<IServiceBusProvider>(),
                        new ServiceProviderMessageHandlerFactory(sp),
                        subscriptionName: subscriptionName ?? AppDomain.CurrentDomain.FriendlyName, // PRODUCT.CAPABILITY
                        filterScope: Environment.GetEnvironmentVariable("ASPNETCORE_ISLOCAL").ToBool()
                            ? Environment.MachineName.Humanize().Dehumanize().ToLower()
                            : string.Empty); // scope the messagebus messages to the local machine, so local events are handled locally.
                });
        }
    }
}
