namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Configuration.Application;
    using Naos.Foundation;
    using Naos.Messaging;
    using Naos.Messaging.Application;
    using Naos.Messaging.Domain;

    [ExcludeFromCodeCoverage]
    public static class NaosExtensions
    {
        public static NaosServicesContextOptions AddMessaging(
            this NaosServicesContextOptions naosOptions,
            Action<MessagingOptions> optionsAction = null)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));
            EnsureArg.IsNotNull(naosOptions.Context, nameof(naosOptions.Context));

            naosOptions.Context.Services.Scan(scan => scan // https://andrewlock.net/using-scrutor-to-automatically-register-your-services-with-the-asp-net-core-di-container/
                .FromExecutingAssembly()
                .FromApplicationDependencies(a => !a.FullName.StartsWithAny(new[] { "Microsoft", "System", "Scrutor", "Consul" }))
                .AddClasses(classes => classes.AssignableTo(typeof(IMessageHandler<>)), true));

            naosOptions.Context.Services.AddSingleton<Hosting.IHostedService>(sp =>
                    new MessagingHostedService(sp.GetRequiredService<ILogger<MessagingHostedService>>(), sp));
            naosOptions.Context.Services.AddSingleton<ISubscriptionMap, SubscriptionMap>();

            optionsAction?.Invoke(new MessagingOptions(naosOptions.Context));

            //context.Messages.Add($"{LogEventKeys.General} naos services builder: messaging added");
            naosOptions.Context.Services.AddSingleton(new NaosFeatureInformation { Name = "Messaging", EchoRoute = "naos/messaging/echo" });

            return naosOptions;
        }
    }
}