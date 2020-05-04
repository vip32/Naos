namespace Naos.Foundation.Domain
{
    using System.Collections.Generic;

    public interface IEventSourcedAggregateRoot<TId>
    {
        long Version { get; }

        void ApplyEvent(IDomainEvent<TId> @event, long version);

        IEnumerable<IDomainEvent<TId>> GetChanges();

        void ClearChanges();
    }
}
