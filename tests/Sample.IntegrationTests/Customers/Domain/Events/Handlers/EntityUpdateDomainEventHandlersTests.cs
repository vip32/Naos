namespace Naos.Sample.IntegrationTests.Customers.Domain
{
    using System;
    using System.Threading.Tasks;
    using MediatR;
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
            var mediator = this.container.GetInstance<IMediator>();
            var entity = new Customer { FirstName = "FirstName1", LastName = "LastName1" };
            entity.State.CreatedDate = new DateTimeEpoch(DateTime.UtcNow.AddDays(-1));
            entity.State.UpdatedDate = null;

            // act
            await mediator.Publish(new EntityUpdateDomainEvent<IEntity>(entity)).ConfigureAwait(false);

            // assert
            entity.State.IdentifierHash.ShouldNotBeNull();
            entity.State.CreatedDate.ShouldNotBeNull();
            entity.State.UpdatedDate.ShouldNotBeNull();
            entity.State.CreatedDate.ShouldNotBe(entity.State.UpdatedDate);
            var hash = entity.State.IdentifierHash;

            entity.FirstName = "FirstName2";
            await mediator.Publish(new EntityUpdateDomainEvent<IEntity>(entity)).ConfigureAwait(false);
            entity.State.IdentifierHash.ShouldNotBe(hash);

            await mediator.Publish(new EntityUpdatedDomainEvent<IEntity>(entity)).ConfigureAwait(false);
            // > CustomerUpdatedDomainEventHandler
        }
    }
}
