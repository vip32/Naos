namespace Naos.Sample.IntegrationTests.Customers.Domain
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
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
            var mediator = this.ServiceProvider.GetService<IMediator>();
            var domainEvent = new StubDomainEvent { Name = "Name1" };
            var entity = new Customer { FirstName = "FirstName1", LastName = "LastName1" };

            // act
            await mediator.Publish(domainEvent).AnyContext();

            // assert
            domainEvent.Properties.ShouldContainKey(typeof(FirstStubDomainEventHandler).Name);
            domainEvent.Properties.ShouldContainKey(typeof(SecondStubDomainEventHandler).Name);
        }

        public class StubDomainEvent : DomainEvent
        {
            public string Name { get; set; }

            public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
        }

        public class FirstStubDomainEventHandler : IDomainEventHandler<StubDomainEvent>
        {
            public bool CanHandle(StubDomainEvent notification)
            {
                return true;
            }

            public Task Handle(StubDomainEvent notification, CancellationToken cancellationToken)
            {
                notification.Properties.AddOrUpdate(this.GetType().Name, true);
                return Task.CompletedTask;
            }
        }

        public class SecondStubDomainEventHandler : IDomainEventHandler<StubDomainEvent>
        {
            public bool CanHandle(StubDomainEvent notification)
            {
                return true;
            }

            public Task Handle(StubDomainEvent notification, CancellationToken cancellationToken)
            {
                notification.Properties.AddOrUpdate(this.GetType().Name, true);
                return Task.CompletedTask;
            }
        }
    }
}
