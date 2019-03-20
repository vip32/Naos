namespace Naos.Core.Messaging.Infrastructure.RabbitMQ
{
    using System;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Messaging.Domain.Model;

    public class RabbitMQMessageBroker : IMessageBroker
    {
        private readonly RabbitMQMessageBrokerOptions options;
        private readonly ILogger<RabbitMQMessageBroker> logger;

        public RabbitMQMessageBroker(RabbitMQMessageBrokerOptions options)
        {
            EnsureThat.EnsureArg.IsNotNull(options, nameof(options));
            EnsureThat.EnsureArg.IsNotNull(options.Host, nameof(options.Host));
            EnsureThat.EnsureArg.IsNotNull(options.HandlerFactory, nameof(options.HandlerFactory));

            this.options = options;
            this.logger = options.LoggerFactory.CreateLogger<RabbitMQMessageBroker>();
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
