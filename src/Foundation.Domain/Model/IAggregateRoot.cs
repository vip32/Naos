namespace Naos.Foundation.Domain
{
    using System.Collections.Generic;

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
    ///   |              |          | -DomainEvents          |
    ///   .--------------.          |                        |
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
    public interface IAggregateRoot<TId>
    {
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        DomainEvents DomainEvents { get; }
    }

    public interface IEventSourcingAggregate<TId>
    {
        long Version { get; }

        void ApplyEvent(IDomainEvent<TId> @event, long version);

        IEnumerable<IDomainEvent<TId>> GetUncommittedEvents();

        void ClearUncommittedEvents();
    }
}
