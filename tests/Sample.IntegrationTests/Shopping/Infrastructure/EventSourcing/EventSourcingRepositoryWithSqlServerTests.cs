#pragma warning disable SA1402 // File may only contain a single type
namespace Naos.Sample.IntegrationTests.Shopping.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using EventStore.ClientAPI;
    using MediatR;
    using Moq;
    using Naos.Foundation;
    using Naos.Foundation.Domain;
    using Naos.Foundation.Infrastructure.EventSourcing;
    using Shouldly;
    using Xunit;
    using Xunit.Abstractions;

    public class EventSourcingRepositoryWithSqlServerTests : BaseTests, IDisposable
    {
        private readonly IEventStoreConnection connection;
        private readonly EventSourcingRepository<TestAggregate, Guid> sut;
        private readonly Mock<IMediator> mediatorMock;
        private readonly ITestOutputHelper output;

        public EventSourcingRepositoryWithSqlServerTests(ITestOutputHelper output)
        {
            this.output = output;
            this.mediatorMock = new Mock<IMediator>();
            this.mediatorMock.Setup(x => x.Publish(It.IsAny<DomainEventBase<Guid>>(), It.IsAny<CancellationToken>())).Returns(It.IsAny<Task>());
            this.connection = EventStoreConnection.Create(new Uri("tcp://localhost:1113"), "naos.test");
            this.connection.ConnectAsync().Wait();
            var eventStore = new EventStoreEventStore(this.connection);
            this.sut = new EventSourcingRepository<TestAggregate, Guid>(eventStore, this.mediatorMock.Object);
        }

        [Fact]
        public async Task EventRoundtripTest()
        {
            // arrange
            //var @event = new TestDomainEvent();
            var aggregate = new TestAggregate("John", "Doe");
            ((IEventSourcedAggregateRoot<Guid>)aggregate).GetChanges().ShouldNotBeEmpty(); // contains TestAggregateCreatedEvent
            ((IEventSourcedAggregateRoot<Guid>)aggregate).Version.ShouldBe(0);

            // act
            await this.sut.SaveAsync(aggregate).AnyContext();
            ((IEventSourcedAggregateRoot<Guid>)aggregate).Version.ShouldBe(0);
            aggregate.ChangeName("Johny Doe01");
            aggregate.ChangeName("Johny Doe02");
            ((IEventSourcedAggregateRoot<Guid>)aggregate).Version.ShouldBe(2);
            aggregate.ChangeName("Johny Doe03");
            ((IEventSourcedAggregateRoot<Guid>)aggregate).GetChanges().ShouldNotBeEmpty();
            await this.sut.SaveAsync(aggregate).AnyContext();
            ((IEventSourcedAggregateRoot<Guid>)aggregate).GetChanges().ShouldBeEmpty();
            ((IEventSourcedAggregateRoot<Guid>)aggregate).Version.ShouldBe(3);

            // assert
            var committedAggregate = await this.sut.GetByIdAsync(aggregate.Id).AnyContext();
            committedAggregate.ShouldNotBeNull();
            committedAggregate.Id.ShouldNotBe(Guid.Empty);
            committedAggregate.FirstName.ShouldBe("Johny");
            committedAggregate.LastName.ShouldBe("Doe03");
            ((IEventSourcedAggregateRoot<Guid>)committedAggregate).GetChanges().ShouldBeEmpty();
            ((IEventSourcedAggregateRoot<Guid>)committedAggregate).Version.ShouldBe(3);
            //aggregate.Changes.ShouldBeEmpty();
            //this.mediatorMock.Verify(x => x.Publish(It.IsAny<TestDomainEvent>(), It.IsAny<CancellationToken>()));
        }

        [Fact]
        public void EventPerformanceTest()
        {
            this.Benchmark(() => this.EventRoundtripTest().Wait(), 100, this.output);
        }

        [Fact]
        public async Task SnapshotRoundtripTest()
        {
            // arrange
            var aggregate = new TestAggregate("John", "Doe");

            // act
            await this.sut.SaveSnapshotAsync(aggregate).AnyContext();
            aggregate.ChangeName("Johny Doe01");
            aggregate.ChangeName("Johny Doe02");
            await this.sut.SaveSnapshotAsync(aggregate).AnyContext();

            // assert
            var snapshotAggregate = await this.sut.GetByIdAsync(aggregate.Id).AnyContext();
            snapshotAggregate.ShouldNotBeNull();
            snapshotAggregate.Id.ShouldNotBe(Guid.Empty);
            snapshotAggregate.FirstName.ShouldBe("Johny");
            snapshotAggregate.LastName.ShouldBe("Doe02");
            ((IEventSourcedAggregateRoot<Guid>)snapshotAggregate).GetChanges().ShouldBeEmpty();

            // act (continue with applying new events)
            snapshotAggregate.ChangeName("Johny Doe03");
            await this.sut.SaveAsync(snapshotAggregate).AnyContext();

            // assert (replay these new events)
            var committedAggregate = await this.sut.GetByIdAsync(aggregate.Id).AnyContext();
            committedAggregate.ShouldNotBeNull();
            committedAggregate.Id.ShouldNotBe(Guid.Empty);
            committedAggregate.FirstName.ShouldBe("Johny");
            committedAggregate.LastName.ShouldBe("Doe03");
        }

        [Fact]
        public void SnapshotPerformanceTest()
        {
            this.Benchmark(() => this.SnapshotRoundtripTest().Wait(), 100, this.output);
        }

        public void Dispose()
        {
            //this.connection.DeleteStreamAsync(this.streamName, ExpectedVersion.Any).Wait();
            this.connection.Dispose();
        }
    }
}