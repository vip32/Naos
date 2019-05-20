namespace Naos.Core.Messaging.Domain
{
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Domain;

    public class DomainEventAsMessagePublisherHandler<TDomainEvent, TMessage> : IDomainEventHandler<IDomainEvent> // handles all domainevents
        where TDomainEvent : class, IDomainEvent
        where TMessage : Message, new()
    {
        private readonly ILogger<DomainEventAsMessagePublisherHandler<TDomainEvent, TMessage>> logger;
        private readonly IMessageBroker messageBroker;
        private readonly IMapper<TDomainEvent, TMessage> mapper;

        public DomainEventAsMessagePublisherHandler(
            ILogger<DomainEventAsMessagePublisherHandler<TDomainEvent, TMessage>> logger,
            IMapper<TDomainEvent, TMessage> mapper,
            IMessageBroker messageBroker)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(mapper, nameof(mapper));
            EnsureArg.IsNotNull(messageBroker, nameof(messageBroker));

            this.logger = logger;
            this.mapper = mapper;
            this.messageBroker = messageBroker;
        }

        public bool CanHandle(IDomainEvent notification)
        {
            return true;
        }

        public Task Handle(IDomainEvent notification, CancellationToken cancellationToken)
        {
            var message = this.mapper.Map(notification as TDomainEvent);
            this.messageBroker.Publish(message);

            return Task.CompletedTask;
        }
    }
}