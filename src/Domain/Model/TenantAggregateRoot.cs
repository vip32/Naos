namespace Naos.Core.Domain
{
    using System.Collections.Generic;

    /// <summary>
    /// <para>
    /// An tenant aggregate root is an entity which works as an entry point to our aggregate.
    /// All business operations + domain events should go through the root. This way, the aggregate root
    /// can take care of keeping the aggregate in a consistent state.
    /// </para>
    /// <para>
    ///
    ///    TenantEntity{TId}
    ///   .-----------------.        IAggregateRoot
    ///   | - Id            |       .------------------------.
    ///   | - TenantId      |       | -DomainEvents()        |
    ///   .-----------------.       | -RegisterDomainEvent() |
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
    public class TenantAggregateRoot<TId> : TenantEntity<TId>, IAggregateRoot
    {
        private readonly ICollection<IDomainEvent> domainEvents = new List<IDomainEvent>();

        public IEnumerable<IDomainEvent> GetDomainEvents() => this.domainEvents;

        public void RegisterDomainEvent(IDomainEvent @event)
        {
            this.domainEvents.Add(@event);
        }

        public void ClearDomainEvents()
        {
            this.domainEvents.Clear();
        }
    }
}
