namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using EnsureThat;
    using Humanizer;
    using MediatR;
    using Microsoft.Azure.Management.ResourceManager.Fluent;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Messaging;
    using Naos.Core.Messaging.App;
    using Naos.Core.Messaging.Domain;
    using Naos.Core.Messaging.Infrastructure.Azure;
    using Naos.Foundation;
    using Naos.Foundation.Infrastructure;

    [ExcludeFromCodeCoverage]
    public static class NaosExtensions
    {
        public static MessagingOptions UseServiceBusBroker(
            this MessagingOptions options,
            Action<IMessageBroker> brokerAction = null,
            string topicName = null,
            string subscriptionName = null,
            string section = "naos:messaging:serviceBus")
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            subscriptionName ??= options.Context.Descriptor.Name;
            var configuration = options.Context.Configuration.GetSection(section).Get<ServiceBusConfiguration>();
            configuration.EntityPath = topicName ?? $"{Environment.GetEnvironmentVariable(EnvironmentKeys.Environment) ?? "Production"}-Naos.Messaging";
            options.Context.Services.AddSingleton<IServiceBusProvider>(sp =>
            {
                if(configuration?.Enabled == true)
                {
                    return new ServiceBusProvider(
                        sp.GetRequiredService<ILogger<ServiceBusProvider>>(),
                        SdkContext.AzureCredentialsFactory.FromServicePrincipal(configuration.ClientId, configuration.ClientSecret, configuration.TenantId, AzureEnvironment.AzureGlobalCloud),
                        configuration);
                }

                throw new NotImplementedException("no messaging servicebus is enabled");
            });

            options.Context.Services.AddSingleton<Azure.ServiceBus.ISubscriptionClient>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<ServiceBusMessageBroker>>();
                var provider = sp.GetRequiredService<IServiceBusProvider>();
                provider.EnsureTopicSubscription(provider.ConnectionStringBuilder.EntityPath, subscriptionName);

                var client = new Azure.ServiceBus.SubscriptionClient(provider.ConnectionStringBuilder, subscriptionName);
                try
                {
                    client
                     .RemoveRuleAsync(RuleDescription.DefaultRuleName)
                     .GetAwaiter()
                     .GetResult();
                }
                catch(MessagingEntityNotFoundException)
                {
                    // do nothing, default rule not found
                }

                client.RegisterMessageHandler(
                    async (m, t) =>
                    {
                        //this.logger.LogInformation("message received (id={MessageId}, name={MessageName})", message.MessageId, message.Label);
                        if(await ServiceBusMessageBroker.ProcessMessage(
                            logger,
                            sp.GetRequiredService<ISubscriptionMap>(),
                            new ServiceProviderMessageHandlerFactory(sp),
                            DefaultSerializer.Create,
                            subscriptionName,
                            (IMediator)sp.CreateScope().ServiceProvider.GetService(typeof(IMediator)),
                            m).AnyContext())
                        {
                            // complete message so it is not received again
                            await client.CompleteAsync(m.SystemProperties.LockToken);
                        }
                    },
                    new MessageHandlerOptions(args =>
                    {
                        var context = args.ExceptionReceivedContext;
                        logger.LogWarning($"{{LogKey:l}} servicebus handler error: topic={context?.EntityPath}, action={context?.Action}, endpoint={context?.Endpoint}, {args.Exception?.Message}, {args.Exception?.StackTrace}", LogKeys.Messaging);
                        return Task.CompletedTask;
                    })
                    {
                        MaxConcurrentCalls = 10,
                        AutoComplete = false,
                        MaxAutoRenewDuration = new TimeSpan(0, 5, 0)
                    });

                return client;
            });

            options.Context.Services.AddSingleton<IMessageBroker>(sp => // TODO: scoped with ITracer injected
            {
                var broker = new ServiceBusMessageBroker(o => o
                    .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                    .Mediator((IMediator)sp.CreateScope().ServiceProvider.GetService(typeof(IMediator)))
                    .Provider(sp.GetRequiredService<IServiceBusProvider>()) // singleton
                    .Client(sp.GetRequiredService<Azure.ServiceBus.ISubscriptionClient>()) // singleton
                    .HandlerFactory(new ServiceProviderMessageHandlerFactory(sp))
                    .Subscriptions(sp.GetRequiredService<ISubscriptionMap>()) // singleton
                    .SubscriptionName(subscriptionName) //AppDomain.CurrentDomain.FriendlyName, // PRODUCT.CAPABILITY
                    //.MessageScope(options.Context.Descriptor.Name)
                    .FilterScope(Environment.GetEnvironmentVariable(EnvironmentKeys.IsLocal).ToBool()
                            ? Environment.MachineName.Humanize().Dehumanize().ToLower()
                            : string.Empty));

                brokerAction?.Invoke(broker);
                return broker;
            }); // scope the messagebus messages to the local machine, so local events are handled locally

            options.Context.Services.AddHealthChecks()
                .AddAzureServiceBusTopic(configuration.ConnectionString, configuration.EntityPath, "messaging-broker-servicebus");

            options.Context.Messages.Add($"{LogKeys.Startup} naos services builder: messaging added (broker={nameof(ServiceBusMessageBroker)})");

            return options;
        }
    }
}
