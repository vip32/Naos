namespace Naos.Foundation.Domain
{
    using System.Collections.Generic;

    public interface IEventSourcingAggregate<TId>
    {
        long Version { get; }

        void ApplyEvent(IDomainEvent<TId> @event, long version);

        IEnumerable<IDomainEvent<TId>> GetUncommittedEvents();

        void ClearUncommittedEvents();
    }
}
