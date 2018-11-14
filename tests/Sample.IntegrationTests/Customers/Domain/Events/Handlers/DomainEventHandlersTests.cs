namespace Naos.Sample.IntegrationTests.Customers.Domain
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Naos.Core.Common;
    using Naos.Core.Domain;
    using Naos.Sample.Customers.Domain;
    using Shouldly;
    using Xunit;

    public class DomainEventHandlersTests : BaseTest
    {
        // TODO: make this more like a real unit tests
        [Fact]
        public async Task DomainEvent_Test()
        {
            // arrange
            var mediator = this.container.GetInstance<IMediator>();
            var domainEvent = new StubDomainEvent { Name = "Name1" };
            var entity = new Customer { FirstName = "FirstName1", LastName = "LastName1" };

            // act
            await mediator.Publish(domainEvent).ConfigureAwait(false);

            // assert
            domainEvent.Properties.ShouldContainKey(typeof(FirstStubDomainEventHandler).Name);
            domainEvent.Properties.ShouldContainKey(typeof(SecondStubDomainEventHandler).Name);
        }

        public class StubDomainEvent : IDomainEvent
        {
            public string Name { get; set; }

            public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
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
