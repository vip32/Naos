namespace Naos.Sample.IntegrationTests.Customers.Domain
{
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Naos.Foundation;
    using Naos.Foundation.Domain;
    using Naos.Sample.Customers.Domain;
    using Shouldly;
    using Xunit;

    public class EntityInsertDomainEventHandlersTests : BaseTests
    {
        [Fact]
        public async Task DomainEvent_Insert_Test()
        {
            // arrange
            var mediator = this.ServiceProvider.GetService<IMediator>();
            var entity = new Customer { FirstName = "FirstName1", LastName = "LastName1" };

            // act
            await mediator.Publish(new EntityInsertDomainEvent(entity)).AnyContext();

            // assert
            entity.IdentifierHash.ShouldNotBeNull();
            entity.State.CreatedDate.ShouldNotBeNull();
            entity.State.UpdatedDate.ShouldNotBeNull();

            await mediator.Publish(new EntityInsertedDomainEvent(entity)).AnyContext();
            // > CustomerInsertedDomainEventHandler
        }
    }
}
