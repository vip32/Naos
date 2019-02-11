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
    using Naos.Core.Messaging.App;
    using Naos.Core.Messaging.App.Web;
    using Naos.Core.Messaging.Infrastructure.Azure;

    public static class ServiceExtensions
    {
        public static MessagingOptions AddServiceBusBroker(
            this MessagingOptions options,
            Action<IMessageBroker> setupAction = null,
            string topicName = null,
            string subscriptionName = null,
            string section = "naos:messaging:serviceBus")
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.Scan(scan => scan // https://andrewlock.net/using-scrutor-to-automatically-register-your-services-with-the-asp-net-core-di-container/
                .FromExecutingAssembly()
                .FromApplicationDependencies(a => !a.FullName.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase) && !a.FullName.StartsWith("System", StringComparison.OrdinalIgnoreCase))
                .AddClasses(classes => classes.AssignableTo(typeof(IMessageHandler<>)), true));

            options.Context.Services.AddSingleton<Hosting.IHostedService>(sp =>
                    new MessagingHostedService(sp.GetRequiredService<ILogger<MessagingHostedService>>(), sp));

            var serviceBusConfiguration = options.Context.Configuration.GetSection(section).Get<ServiceBusConfiguration>();
            serviceBusConfiguration.EntityPath = topicName ?? $"{Environment.GetEnvironmentVariable(EnvironmentKeys.Environment) ?? "Production"}-Naos.Messaging";
            options.Context.Services.AddSingleton<IServiceBusProvider>(sp =>
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

            options.Context.Services.AddSingleton<ISubscriptionMap, SubscriptionMap>();
            options.Context.Services.AddSingleton<IMessageBroker>(sp =>
            {
                var result = new ServiceBusMessageBroker(o => o
                    .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                    .Mediator((IMediator)sp.CreateScope().ServiceProvider.GetService(typeof(IMediator)))
                    .Provider(sp.GetRequiredService<IServiceBusProvider>())
                    .HandlerFactory(new ServiceProviderMessageHandlerFactory(sp))
                    .Map(sp.GetRequiredService<ISubscriptionMap>())
                    .SubscriptionName(subscriptionName ?? options.Context.Descriptor.Name) //AppDomain.CurrentDomain.FriendlyName, // PRODUCT.CAPABILITY
                    .FilterScope(Environment.GetEnvironmentVariable(EnvironmentKeys.IsLocal).ToBool()
                            ? Environment.MachineName.Humanize().Dehumanize().ToLower()
                            : string.Empty));

                setupAction?.Invoke(result);
                return result;
            }); // scope the messagebus messages to the local machine, so local events are handled locally

            options.Context.Services.AddHealthChecks()
                .AddAzureServiceBusTopic(serviceBusConfiguration.ConnectionString, serviceBusConfiguration.EntityPath, "messaging-servicebus");

            return options;
        }
    }
}
