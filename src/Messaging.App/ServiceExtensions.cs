namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using Naos.Core.Messaging.App;

    public static class ServiceExtensions
    {
        public static INaosBuilder AddMessaging(
            this INaosBuilder context,
            Action<MessagingOptions> setupAction = null)
        {
            setupAction?.Invoke(new MessagingOptions(context));

            return context;
        }
    }
}