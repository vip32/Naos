namespace Naos.Core.Messaging.Infrastructure.Azure
{
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Messaging.Domain.Model;

    public class StorageQueueMessageBroker : IMessageBroker
    {
        private readonly ILogger<StorageQueueMessageBroker> logger;
        private readonly string storageConnectionString;
        private readonly ISubscriptionMap map;
        private readonly IMessageHandlerFactory handlerFactory;

        public StorageQueueMessageBroker(
            ILogger<StorageQueueMessageBroker> logger,
            string storageConnectionString,
            IMessageHandlerFactory handlerFactory,
            ISubscriptionMap map = null)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNullOrEmpty(storageConnectionString, nameof(storageConnectionString));
            EnsureArg.IsNotNull(handlerFactory, nameof(handlerFactory));

            this.logger = logger;
            this.storageConnectionString = storageConnectionString;
            this.map = map ?? new SubscriptionMap();
            this.handlerFactory = handlerFactory;

            // maybe not a good fit for a message broker
            // https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-azure-and-service-bus-queues-compared-contrasted
            // - no push style api (no onmessage())
            // - single subscriber only (due to message.dequeue), has peek however
        }

        public void Publish(Message message)
        {
            throw new System.NotImplementedException();
        }

        public IMessageBroker Subscribe<TMessage, THandler>()
            where TMessage : Message
            where THandler : IMessageHandler<TMessage>
        {
            return this;
        }

        public void Unsubscribe<TMessage, THandler>()
            where TMessage : Message
            where THandler : IMessageHandler<TMessage>
        {
            throw new System.NotImplementedException();
        }
    }
}
