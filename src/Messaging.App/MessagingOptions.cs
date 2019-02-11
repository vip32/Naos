namespace Naos.Core.Messaging.App
{
    using Microsoft.Extensions.DependencyInjection;

    public class MessagingOptions
    {
        public MessagingOptions(INaosBuilder context)
        {
            this.Context = context;
        }

        public INaosBuilder Context { get; }
    }
}
