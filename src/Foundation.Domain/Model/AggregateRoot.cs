namespace Naos.Foundation.Domain
{
    using System.Diagnostics;

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
    [DebuggerDisplay("Type={GetType().Name}, Id={Id}")]
    public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot
    {
        protected AggregateRoot()
        {
            this.DomainEvents = new DomainEvents();
        }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public DomainEvents DomainEvents { get; }
    }
}
