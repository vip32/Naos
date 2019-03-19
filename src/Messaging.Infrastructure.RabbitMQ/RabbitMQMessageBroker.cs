namespace Naos.Core.Messaging.Infrastructure.RabbitMQ
{
    using System;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Messaging.Domain.Model;

    public class RabbitMQMessageBroker : IMessageBroker
    {
        private readonly ILogger<RabbitMQMessageBroker> logger;
        private readonly RabbitMQConfiguration configuration;
        private readonly IMessageHandlerFactory handlerFactory;

        public RabbitMQMessageBroker(RabbitMQMessageBrokerOptions options)
        {
            EnsureThat.EnsureArg.IsNotNull(options.LoggerFactory, nameof(options.LoggerFactory));
            EnsureThat.EnsureArg.IsNotNull(options.Configuration, nameof(options.Configuration));
            EnsureThat.EnsureArg.IsNotNull(options.HandlerFactory, nameof(options.HandlerFactory));

            this.logger = options.LoggerFactory.CreateLogger<RabbitMQMessageBroker>();
            this.configuration = options.Configuration;
            this.handlerFactory = options.HandlerFactory;
        }

        public RabbitMQMessageBroker(Builder<RabbitMQMessageBrokerOptionsBuilder, RabbitMQMessageBrokerOptions> optionsBuilder)
            : this(optionsBuilder(new RabbitMQMessageBrokerOptionsBuilder()).Build())
        {
        }

        public void Publish(Message message)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }
    }
}
