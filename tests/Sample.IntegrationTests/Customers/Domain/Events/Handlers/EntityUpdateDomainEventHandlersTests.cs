namespace Naos.Sample.IntegrationTests.Customers.Domain
{
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Naos.Core.Common;
    using Naos.Core.Domain;
    using Naos.Sample.Customers.Domain;
    using Shouldly;
    using Xunit;

    public class EntityUpdateDomainEventHandlersTests : BaseTest
    {
        [Fact]
        public async Task DomainEvent_Update_Test()
        {
            // arrange
            var mediator = this.ServiceProvider.GetService<IMediator>();
            var entity = new Customer { FirstName = "FirstName1", LastName = "LastName1" };
            entity.State.SetCreated();
            //entity.State.CreatedDate = DateTime.UtcNow.AddDays(-1);

            // act
            await mediator.Publish(new EntityUpdateDomainEvent(entity)).AnyContext();

            // assert
            entity.IdentifierHash.ShouldNotBeNull();
            entity.State.CreatedDate.ShouldNotBeNull();
            entity.State.UpdatedDate.ShouldNotBeNull();
            entity.State.CreatedDate.ShouldNotBe(entity.State.UpdatedDate);
            var hash = entity.IdentifierHash;

            entity.FirstName = "FirstName2";
            await mediator.Publish(new EntityUpdateDomainEvent(entity)).AnyContext();
            entity.IdentifierHash.ShouldNotBe(hash);

            await mediator.Publish(new EntityUpdatedDomainEvent(entity)).AnyContext();
            // > CustomerUpdatedDomainEventHandler
        }
    }
}
