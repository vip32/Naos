namespace Naos.Core.Domain
{
    using System.Collections.Generic;
    using EnsureThat;

    /// <summary>
    /// <para>
    /// An aggregate root is an entity which works as an entry point to our aggregate.
    /// All business operations + domain events should go through the root. This way, the aggregate root
    /// can take care of keeping the aggregate in a consistent state.
    /// </para>
    /// <para>
    ///
    ///    Entity{TId}
    ///   .--------------.           IAggregateRoot
    ///   | - Id         |          .------------------------.
    ///   |              |          | -DomainEvents()        |
    ///   .--------------.          | -RegisterDomainEvent() |
    ///              /`\            .------------------------.
    ///               | inherits          /`\
    ///               |                    | implements
    ///        AggregateRoot{TId}         /
    ///       .------------------.       /
    ///       |                  |______/
    ///       |                  |
    ///       |                  |
    ///       .------------------.
    ///
    /// </para>
    /// </summary>
    public class AggregateRoot<TId> : Entity<TId>, IAggregateRoot
    {
        /// <summary>
        /// The domain events.
        /// </summary>
        private readonly ICollection<IDomainEvent> domainEvents = new List<IDomainEvent>();

        public IEnumerable<IDomainEvent> GetDomainEvents() => this.domainEvents;

        /// <summary>
        /// Registers the domain event.
        /// Domain Events are only registered on the aggregate root because it is ensuring the integrity of the aggregate as a whole.
        /// </summary>
        /// <param name="event">The event.</param>
        public void RegisterDomainEvent(IDomainEvent @event)
        {
            EnsureArg.IsNotNull(@event, nameof(@event));

            this.domainEvents.Add(@event);
        }

        /// <summary>
        /// Clears the domain events.
        /// </summary>
        /// <param name="event">The event.</param>
        public void ClearDomainEvents()
        {
            this.domainEvents.Clear();
        }
    }
}
