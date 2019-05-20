namespace Naos.Core.UnitTests.Messaging.Domain.Events.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Domain;
    using Naos.Core.Messaging;
    using Naos.Core.Messaging.Domain;
    using Naos.Core.UnitTests.Domain.Repositories;
    using NSubstitute;
    using Xunit;

    public class DomainEventMessagingPublisherHandlerTests
    {
        [Fact]
        public async Task CanMap_Test()
        {
            // arrange
            var messageBroker = Substitute.For<IMessageBroker>();
            var domainEvent = new CustomerCreatedDomainEvent(new StubEntity { FirstName = "John", LastName = "Doe", Id = "112233", Age = 25 });
            var sut = new CustomerCreatedDomainEventMessagingPublisher(
                Substitute.For<ILogger<CustomerCreatedDomainEventMessagingPublisher>>(),
                messageBroker);

            // act
            await sut.Handle(domainEvent, CancellationToken.None);

            // assert
            messageBroker.Received().Publish(Arg.Is<CustomerCreatedMessage>(m => m.FullName == "John Doe"));
        }

        public class CustomerCreatedDomainEvent : DomainEvent // internal event
        {
            public CustomerCreatedDomainEvent(StubEntity entity)
            {
                this.Entity = entity;
            }

            public StubEntity Entity { get; }
        }

        public class CustomerCreatedMessage : Message // external event
        {
            public string FullName { get; set; }
        }

        public class CustomerCreatedDomainEventMessagingPublisher : DomainEventMessagingPublisher<CustomerCreatedDomainEvent, CustomerCreatedMessage>
        {
            public CustomerCreatedDomainEventMessagingPublisher(
                ILogger<CustomerCreatedDomainEventMessagingPublisher> logger,
                IMessageBroker messageBroker)
                : base(
                    logger,
                    new Mapper<CustomerCreatedDomainEvent, CustomerCreatedMessage>((s, d) => d.FullName = $"{s.Entity.FirstName} {s.Entity.LastName}"),
                    messageBroker)
            {
            }
        }
    }
}
