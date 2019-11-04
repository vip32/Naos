namespace Naos.Messaging.Application
{
    using Microsoft.Extensions.DependencyInjection;

    public class MessagingOptions
    {
        public MessagingOptions(INaosBuilderContext context)
        {
            this.Context = context;
        }

        public INaosBuilderContext Context { get; }
    }
}
