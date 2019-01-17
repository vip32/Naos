namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using EnsureThat;
    using Humanizer;
    using MediatR;
    using Microsoft.Azure.Management.ResourceManager.Fluent;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Infrastructure.Azure.ServiceBus;
    using Naos.Core.Messaging;
    using Naos.Core.Messaging.App.Web;
    using Naos.Core.Messaging.Infrastructure.Azure.ServiceBus;

    public static class ServiceExtensions
    {
        public static IServiceCollection AddNaosMessagingAzureServiceBus(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<IMessageBroker> setupAction = null,
            string topicName = null,
            string subscriptionName = null,
            string section = "naos:messaging:serviceBus")
        {
            EnsureArg.IsNotNull(services, nameof(services));

            services.Scan(scan => scan // https://andrewlock.net/using-scrutor-to-automatically-register-your-services-with-the-asp-net-core-di-container/
                .FromExecutingAssembly()
                .FromApplicationDependencies(a => !a.FullName.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase) && !a.FullName.StartsWith("System", StringComparison.OrdinalIgnoreCase))
                .AddClasses(classes => classes.AssignableTo(typeof(IMessageHandler<>)), true));

            services.AddSingleton<Hosting.IHostedService>(sp =>
                    new MessagingHostedService(sp.GetRequiredService<ILogger<MessagingHostedService>>(), sp));

            var serviceBusConfiguration = configuration.GetSection(section).Get<ServiceBusConfiguration>();
            serviceBusConfiguration.EntityPath = topicName ?? $"{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}-Naos.Messaging";
            services.AddSingleton<IServiceBusProvider>(sp =>
            {
                if (serviceBusConfiguration?.Enabled == true)
                {
                    return new ServiceBusProvider(
                        sp.GetRequiredService<ILogger<ServiceBusProvider>>(),
                        SdkContext.AzureCredentialsFactory.FromServicePrincipal(serviceBusConfiguration.ClientId, serviceBusConfiguration.ClientSecret, serviceBusConfiguration.TenantId, AzureEnvironment.AzureGlobalCloud),
                        serviceBusConfiguration);
                }

                throw new NotImplementedException("no messaging servicebus is enabled");
            });

            services.AddSingleton<ISubscriptionMap, SubscriptionMap>();
            services.AddSingleton<IMessageBroker>(sp =>
            {
                // HACK: get a registerd as scoped instance (mediator) inside a singleton instance
                IMediator mediator = null;
                //using (var scope = sp.CreateScope())
                //{
                //    mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                //}

                mediator = sp.CreateScope().ServiceProvider.GetRequiredService<IMediator>(); // WARN: is not disposed

                var result = new ServiceBusMessageBroker(
                        sp.GetRequiredService<ILogger<ServiceBusMessageBroker>>(),
                        mediator,
                        sp.GetRequiredService<IServiceBusProvider>(),
                        new ServiceProviderMessageHandlerFactory(sp),
                        map: sp.GetRequiredService<ISubscriptionMap>(),
                        subscriptionName: subscriptionName ?? AppDomain.CurrentDomain.FriendlyName, // PRODUCT.CAPABILITY
                        filterScope: Environment.GetEnvironmentVariable("ASPNETCORE_ISLOCAL").ToBool()
                            ? Environment.MachineName.Humanize().Dehumanize().ToLower()
                            : string.Empty);

                setupAction?.Invoke(result);
                return result;
            }); // scope the messagebus messages to the local machine, so local events are handled locally

            services.AddHealthChecks()
                .AddAzureServiceBusTopic(serviceBusConfiguration.ConnectionString, serviceBusConfiguration.EntityPath, "messaging-servicebus");

            return services;
        }
    }
}
