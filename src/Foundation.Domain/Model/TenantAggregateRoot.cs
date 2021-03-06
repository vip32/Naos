﻿namespace Naos.Foundation.Domain
{
    using System.Diagnostics;

    /// <summary>
    /// <para>
    /// An tenant aggregate root is an entity which works as an entry point to our aggregate.
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
    [DebuggerDisplay("Type={GetType().Name}, Id={Id}, Tenant={TenantId}")]
    public abstract class TenantAggregateRoot<TId> : TenantEntity<TId>, IAggregateRoot
    {
        public DomainEvents DomainEvents => new DomainEvents();
    }
}
