namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using Naos.Core.Messaging.App;

    public static class ServiceExtensions
    {
        public static ServiceConfigurationContext AddMessaging(
            this ServiceConfigurationContext context,
            Action<MessagingOptions> setupAction)
        {
            setupAction?.Invoke(new MessagingOptions(context));

            return context;
        }
    }
}