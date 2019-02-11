namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Messaging;
    using Naos.Core.Messaging.App;

    public static class ServiceExtensions
    {
        public static INaosBuilder AddMessaging(
            this INaosBuilder context,
            Action<MessagingOptions> setupAction = null)
        {
            context.Services.Scan(scan => scan // https://andrewlock.net/using-scrutor-to-automatically-register-your-services-with-the-asp-net-core-di-container/
                .FromExecutingAssembly()
                .FromApplicationDependencies(a => !a.FullName.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase) && !a.FullName.StartsWith("System", StringComparison.OrdinalIgnoreCase))
                .AddClasses(classes => classes.AssignableTo(typeof(IMessageHandler<>)), true));

            context.Services.AddSingleton<Hosting.IHostedService>(sp =>
                    new MessagingHostedService(sp.GetRequiredService<ILogger<MessagingHostedService>>(), sp));
            context.Services.AddSingleton<ISubscriptionMap, SubscriptionMap>();

            setupAction?.Invoke(new MessagingOptions(context));

            return context;
        }
    }
}