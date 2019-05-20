namespace Naos.Core.Messaging.Domain
{
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Domain;

    /// <summary>
    /// <para>
    /// A domain event handler which can publish the domain events as messaging messages.
    /// Can be used to expose internal domain events as external messages, for example to notify other services.
    /// </para>
    /// <para>
    ///     .----.                                                                       .
    ///     | a  |    .----.                              .----------.                  /
    ///     "----"    | c  |             .--------.       | Domain   |                 /
    ///          |    "----"   x-------> | Domain |       | Event    |                /
    ///        .----.   /                | Event  x-----> | PUBLISHER|            .---------.
    ///        | b  |--"                 "--------"       | Handler  |            | Message |
    ///        "----"                                     |----------|            | Broker  |
    ///      Domain Model                                 |-Handle() x----------> |         |                  External Service
    ///                                                   |          |  Publish() |         |              .-----------------------.
    ///                                                   |          |            "----x----"             / .----. Domain Model   /
    ///                                                   "----------"          /      |                 /  | x  |    .----.     /
    ///                                                                        /       "--------------> /   "----"    | y  |    /
    ///        Internal Service (origin                                       /              subscribed/         |    "----"   /
    ///     -----------------------------------------------------------------"                        /        .----.   /     /
    ///                                                                                              /         |  z |--"     /
    ///                                                                                             /          "----"       /
    ///                                                                                            "-----------------------"
    /// </para>
    ///
    /// </summary>
    /// <typeparam name="TDomainEvent"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    public abstract class DomainEventAsMessagePublisherHandler<TDomainEvent, TMessage> : IDomainEventHandler<IDomainEvent> // handles all domainevents
        where TDomainEvent : class, IDomainEvent
        where TMessage : Message, new()
    {
        private readonly ILogger<DomainEventAsMessagePublisherHandler<TDomainEvent, TMessage>> logger;
        private readonly IMessageBroker messageBroker;
        private readonly IMapper<TDomainEvent, TMessage> mapper;

        protected DomainEventAsMessagePublisherHandler(
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

        public virtual bool CanHandle(IDomainEvent notification)
        {
            return true;
        }

        public virtual Task Handle(IDomainEvent notification, CancellationToken cancellationToken)
        {
            var message = this.mapper.Map(notification as TDomainEvent);
            this.messageBroker.Publish(message);

            return Task.CompletedTask;
        }
    }
}