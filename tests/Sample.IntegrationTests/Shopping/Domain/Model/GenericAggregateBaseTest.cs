namespace Naos.Sample.IntegrationTests.Shopping.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Naos.Foundation.Domain;
    using Xunit;

    public abstract class GenericAggregateBaseTest<TAggregate, TAggregateId>
        where TAggregate : EventSourcingAggregateRoot<TAggregateId>, IEventSourcingAggregate<TAggregateId>
        //where TAggregateId : IAggregateId
    {
        protected void AssertSingleUncommittedEventOfType<TEvent>(TAggregate aggregate)
            where TEvent : IDomainEvent<TAggregateId>
        {
            var uncommittedEvents = this.GetUncommittedEventsOf(aggregate);

            Assert.Single(uncommittedEvents);
            Assert.IsType<TEvent>(uncommittedEvents.First());
        }

        protected void AssertSingleUncommittedEvent<TEvent>(TAggregate aggregate, Action<TEvent> assertions)
            where TEvent : IDomainEvent<TAggregateId>
        {
            this.AssertSingleUncommittedEventOfType<TEvent>(aggregate);
            assertions((TEvent)((IEventSourcingAggregate<TAggregateId>)aggregate).GetUncommittedEvents().Single());
        }

        protected void ClearUncommittedEvents(TAggregate aggregate)
        {
            ((IEventSourcingAggregate<TAggregateId>)aggregate).ClearUncommittedEvents();
        }

        protected IEnumerable<IDomainEvent<TAggregateId>> GetUncommittedEventsOf(TAggregate aggregate)
        {
            return ((IEventSourcingAggregate<TAggregateId>)aggregate).GetUncommittedEvents();
        }
    }
}
