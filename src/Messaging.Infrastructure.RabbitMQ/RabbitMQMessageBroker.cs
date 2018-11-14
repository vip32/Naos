namespace Naos.Core.Messaging.Infrastructure.RabbitMQ
{
    using System;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Messaging.Domain.Model;

    public class RabbitMQMessageBroker : IMessageBroker
    {
        private readonly ILogger<RabbitMQMessageBroker> logger;
        private readonly RabbitMQConfiguration configuration;
        private readonly IMessageHandlerFactory handlerFactory;

        public RabbitMQMessageBroker(ILogger<RabbitMQMessageBroker> logger, RabbitMQConfiguration configuration, IMessageHandlerFactory handlerFactory)
        {
            EnsureThat.EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureThat.EnsureArg.IsNotNull(configuration, nameof(configuration));
            EnsureThat.EnsureArg.IsNotNull(handlerFactory, nameof(handlerFactory));

            this.logger = logger;
            this.configuration = configuration;
            this.handlerFactory = handlerFactory;
        }

        public void Publish(Message message)
        {
            throw new NotImplementedException();
        }

        public void Subscribe<TMessage, THandler>()
            where TMessage : Message
            where THandler : IMessageHandler<TMessage>
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe<TMessage, THandler>()
            where TMessage : Message
            where THandler : IMessageHandler<TMessage>
        {
            throw new NotImplementedException();
        }
    }
}
