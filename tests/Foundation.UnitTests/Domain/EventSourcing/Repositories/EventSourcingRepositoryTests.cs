namespace Naos.Foundation.UnitTests.Domain.EventSourcing
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Moq;
    using Naos.Foundation.Domain;
    using Naos.Foundation.Domain.EventSourcing;
    using Xunit;

#pragma warning disable SA1402 // File may only contain a single type
    public class EventSourcingRepositoryTests
    {
        private static readonly Guid DefaultId = Guid.NewGuid();
        private readonly IEventSourcingRepository<TestAggregate, Guid> sut;
        private readonly Mock<IEventStore> eventStoreMock;
        private readonly Mock<IMediator> mediatorMock;

        public EventSourcingRepositoryTests()
        {
            this.mediatorMock = new Mock<IMediator>();
            this.eventStoreMock = new Mock<IEventStore>();
            this.sut = new EventSourcingRepository<TestAggregate, Guid>(
                this.eventStoreMock.Object,
                this.mediatorMock.Object);
        }

        [Fact]
        public async Task ShouldLoadAnAggregateAndApplyEventsAsync()
        {
            var domainEvent = new TestDomainEvent();
            this.eventStoreMock.Setup(x => x.ReadEventsAsync<Guid>($"TestAggregate-{DefaultId}", null, null))
                .ReturnsAsync(new List<Event<Guid>>()
                {
                    new Event<Guid>(domainEvent, 0)
                });
            var aggregate = await this.sut.GetByIdAsync(DefaultId).AnyContext();

            Assert.NotNull(aggregate);
            Assert.Single(aggregate.AppliedEvents);
            Assert.Equal(domainEvent, aggregate.AppliedEvents[0]);
        }

        [Fact]
        public async Task ShouldPublishUncommittedEventsOnSaveAsync()
        {
            // arrange
            var @event = new TestDomainEvent();
            var aggregate = new TestAggregate(@event);
            this.mediatorMock.Setup(x => x.Publish(It.IsAny<TestDomainEvent>(), It.IsAny<CancellationToken>())).Returns(It.IsAny<Task>());
            this.eventStoreMock.Setup(x => x.SaveEventAsync($"TestAggregate-{@event.AggregateId}", @event)).ReturnsAsync(new EventResult(1));

            // act
            await this.sut.SaveAsync(aggregate).AnyContext();

            // assert
            this.eventStoreMock.Verify(x => x.SaveEventAsync(It.IsAny<string>(), It.IsAny<TestDomainEvent>()));
            //this.mediatorMock.Verify(x => x.Publish(It.IsAny<TestDomainEvent>(), It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task ShouldReturnsNullWhenAggregateNotFoundOrDeletedAsync()
        {
            this.eventStoreMock.Setup(x => x.ReadEventsAsync<Guid>($"TestAggregate-{DefaultId}", null, null)).Throws<EventStoreStreamNotFoundException>();
            Assert.Null(await this.sut.GetByIdAsync(DefaultId).AnyContext());
        }

        [Fact]
        public void ShouldThrowsExceptionWhenEventStoreHasCommunicationIssues()
        {
            this.eventStoreMock.Setup(x => x.ReadEventsAsync<Guid>($"TestAggregate-{DefaultId}", null, null)).Throws<EventStoreCommunicationException>();
            Assert.ThrowsAsync<EventSourcingRepositoryException>(async () => { await this.sut.GetByIdAsync(DefaultId).AnyContext(); });
        }
    }

    public class TestAggregate : EventSourcedAggregateRoot<Guid>
    {
#pragma warning disable CA1051 // Do not declare visible instance fields
        public readonly List<IDomainEvent<Guid>> AppliedEvents = new List<IDomainEvent<Guid>>();
#pragma warning restore CA1051 // Do not declare visible instance fields

        public TestAggregate(params TestDomainEvent[] events)
        {
            foreach (var @event in events)
            {
                this.RaiseEvent(@event);
            }
        }

        private TestAggregate()
        {
        }

        public void Apply(TestDomainEvent @event)
        {
            this.AppliedEvents.Add(@event);
        }
    }

    public class TestDomainEvent : DomainEventBase<Guid>
    {
        //public override IDomainEvent<Guid> WithAggregate(Guid aggregateId, long aggregateVersion)
        //{
        //    return this;
        //}
    }
}
