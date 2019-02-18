namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Messaging;
    using Naos.Core.Messaging.App;

    public static class NaosExtensions
    {
        public static NaosOptions AddMessaging(
            this NaosOptions naosOptions,
            Action<MessagingOptions> setupAction = null)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));
            EnsureArg.IsNotNull(naosOptions.Context, nameof(naosOptions.Context));

            naosOptions.Context.Services.Scan(scan => scan // https://andrewlock.net/using-scrutor-to-automatically-register-your-services-with-the-asp-net-core-di-container/
                .FromExecutingAssembly()
                .FromApplicationDependencies(a => !a.FullName.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase) && !a.FullName.StartsWith("System", StringComparison.OrdinalIgnoreCase))
                .AddClasses(classes => classes.AssignableTo(typeof(IMessageHandler<>)), true));

            naosOptions.Context.Services.AddSingleton<Hosting.IHostedService>(sp =>
                    new MessagingHostedService(sp.GetRequiredService<ILogger<MessagingHostedService>>(), sp));
            naosOptions.Context.Services.AddSingleton<ISubscriptionMap, SubscriptionMap>();

            setupAction?.Invoke(new MessagingOptions(naosOptions.Context));

            //context.Messages.Add($"{LogEventKeys.General} naos builder: messaging added");

            return naosOptions;
        }
    }
}