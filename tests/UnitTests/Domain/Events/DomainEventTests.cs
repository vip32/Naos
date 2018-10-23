namespace Naos.Core.UnitTests.Domain.Events
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Naos.Core.Common;
    using Naos.Core.Common.Dependency.SimpleInjector;
    using Naos.Core.Domain;
    using SimpleInjector;
    using Xunit;

    public class DomainEventTests
    {
        // TODO: make this more like a real unit tests
        [Fact]
        public async Task DomainEventIsHandledByDomainEventHandlersTest()
        {
            // arrange
            var container = new Container();
            container.BuildMediator(new[] { typeof(IEntity).Assembly, typeof(DomainEventTests).Assembly });
            var mediator = container.GetInstance<IMediator>();

            var domainEvent = new StubDomainEvent { Name = "Name1" };
            var entity = new StubEntity { FirstName = "FirstName1", LastName = "LastName1" };
            entity.State.CreatedDate = null;
            entity.State.UpdatedDate = null;
            var createEvent = new EntityCreateDomainEvent<IEntity>(entity);
            var createdEvent = new EntityCreatedDomainEvent<IEntity>(entity);
            var updateEvent = new EntityUpdateDomainEvent<IEntity>(entity);
            var updatedEvent = new EntityUpdatedDomainEvent<IEntity>(entity);

            // act/assert
            await mediator.Publish(domainEvent).ConfigureAwait(false);
            Assert.True(domainEvent.Properties.ContainsKey(typeof(FirstStubDomainEventHandler).Name));
            Assert.True(domainEvent.Properties.ContainsKey(typeof(SecondStubDomainEventHandler).Name));

            await mediator.Publish(createEvent).ConfigureAwait(false);
            Assert.NotNull(entity.VersionIdentifier);
            Assert.True(entity.State.CreatedDate != null);
            var version = entity.VersionIdentifier;
            Assert.NotNull(entity.VersionIdentifier);

            await mediator.Publish(createdEvent).ConfigureAwait(false);

            Thread.Sleep(1000);
            await mediator.Publish(updateEvent).ConfigureAwait(false);
            Assert.True(entity.State.UpdatedDate != null);
            Assert.True(entity.State.CreatedDate != entity.State.UpdatedDate);
            Assert.NotNull(entity.VersionIdentifier);
            Assert.NotEqual(version, entity.VersionIdentifier);
        }

        public class StubDomainEvent : IDomainEvent
        {
            public string Name { get; set; }

            public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
        }

        public class StubEntity : Entity<string>
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }
        }

        public class FirstStubDomainEventHandler : IDomainEventHandler<StubDomainEvent>
        {
            public Task Handle(StubDomainEvent notification, CancellationToken cancellationToken)
            {
                notification.Properties.AddOrUpdate(this.GetType().Name, true);
                return Task.CompletedTask;
            }
        }

        public class SecondStubDomainEventHandler : IDomainEventHandler<StubDomainEvent>
        {
            public Task Handle(StubDomainEvent notification, CancellationToken cancellationToken)
            {
                notification.Properties.AddOrUpdate(this.GetType().Name, true);
                return Task.CompletedTask;
            }
        }
    }
}
