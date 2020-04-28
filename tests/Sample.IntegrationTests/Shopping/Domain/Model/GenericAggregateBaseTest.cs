namespace Naos.Sample.IntegrationTests.Shopping.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Naos.Foundation.Domain;
    using Xunit;

    public abstract class GenericAggregateBaseTest<TAggregate, TId>
        where TAggregate : EventSourcingAggregateRoot<TId>, IEventSourcingAggregate<TId>
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
            assertions((TEvent)aggregate.GetUncommittedEvents().Single());
        }

        protected void ClearUncommittedEvents(TAggregate aggregate)
        {
            aggregate.ClearUncommittedEvents();
        }

        protected IEnumerable<IDomainEvent<TId>> GetUncommittedEventsOf(TAggregate aggregate)
        {
            return aggregate.GetUncommittedEvents();
        }
    }
}
