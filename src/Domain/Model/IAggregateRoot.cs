﻿namespace Naos.Core.Domain
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
    public interface IAggregateRoot
    {
        /// <summary>
        /// Gets the domain events.
        /// </summary>
        /// <value>
        /// The domain events.
        /// </value>
        IEnumerable<IDomainEvent> GetDomainEvents();

        /// <summary>
        /// Registers the domain event.
        /// Domain Events are only registered on the aggregate root because it is ensuring the integrity of the aggregate as a whole.
        /// </summary>
        /// <param name="event">The event.</param>
        void RegisterDomainEvent(IDomainEvent @event);

        /// <summary>
        /// Clears the domain events.
        /// </summary>
        /// <param name="event">The event.</param>
        void ClearDomainEvents();
    }
}
