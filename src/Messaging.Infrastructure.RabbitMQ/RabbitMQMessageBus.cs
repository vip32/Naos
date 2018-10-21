namespace Naos.Core.Messaging.Infrastructure.RabbitMQ
{
    using System;
    using Naos.Core.Messaging.Domain.Model;

    public class RabbitMQMessageBus : IMessageBus
    {
        private readonly RabbitMQConfiguration configuration;

        public RabbitMQMessageBus(RabbitMQConfiguration configuration)
        {
            EnsureThat.EnsureArg.IsNotNull(configuration, nameof(configuration));

            this.configuration = configuration;
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
