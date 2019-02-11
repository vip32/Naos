namespace Naos.Core.Messaging.App
{
    using Microsoft.Extensions.DependencyInjection;

    public class MessagingOptions
    {
        public MessagingOptions(ServiceConfigurationContext context)
        {
            this.Context = context;
        }

        public ServiceConfigurationContext Context { get; }
    }
}
