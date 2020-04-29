namespace Naos.Sample.IntegrationTests.Shopping.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Naos.Foundation.Domain;
    using Xunit;

    public abstract class GenericAggregateBaseTest<TAggregate, TId>
        where TAggregate : EventSourcedAggregateRoot<TId>, IEventSourcedAggregateRoot<TId>
    {
        protected void AssertSingleUncommittedEventOfType<TEvent>(TAggregate aggregate)
            where TEvent : IDomainEvent<TId>
        {
            var events = this.GetUncommittedEventsOf(aggregate);

            Assert.Single(events);
            Assert.IsType<TEvent>(events.First());
        }

        protected void AssertSingleUncommittedEvent<TEvent>(TAggregate aggregate, Action<TEvent> assertions)
            where TEvent : IDomainEvent<TId>
        {
            this.AssertSingleUncommittedEventOfType<TEvent>(aggregate);
            assertions((TEvent)aggregate.GetChanges().Single());
        }

        protected void ClearUncommittedEvents(TAggregate aggregate)
        {
            aggregate.ClearChanges();
        }

        protected IEnumerable<IDomainEvent<TId>> GetUncommittedEventsOf(TAggregate aggregate)
        {
            return aggregate.GetChanges();
        }
    }
}
