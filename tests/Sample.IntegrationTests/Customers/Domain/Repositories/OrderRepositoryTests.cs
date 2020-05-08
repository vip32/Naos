namespace Naos.Sample.IntegrationTests.Customers.Domain
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Bogus;
    using Microsoft.Extensions.DependencyInjection;
    using Naos.Foundation;
    using Naos.Foundation.Domain;
    using Naos.Sample.Customers.Domain;
    using Shouldly;
    using Xunit;

    public class OrderRepositoryTests : BaseTests
    {
        // https://xunit.github.io/docs/shared-context.html
        private readonly IOrderRepository sut;
        private readonly Faker<Order> entityFaker;
        private readonly string tenantId = "naos_sample_test";

        public OrderRepositoryTests()
        {
            this.sut = this.ServiceProvider.GetService<IOrderRepository>();
            this.entityFaker = new Faker<Order>() //https://github.com/bchavez/Bogus
                //.RuleFor(u => u.CustomerNumber, f => f.Random.Replace("??-#####"))
                .RuleFor(u => u.OrderNumber, f => f.Random.AlphaNumeric(8))
                .RuleFor(u => u.FirstName, (f, u) => f.Name.FirstName())
                .RuleFor(u => u.LastName, (f, u) => f.Name.LastName())
                .RuleFor(u => u.Total, (f, u) => f.Random.Decimal())
                .RuleFor(u => u.Region, (f, u) => f.PickRandom(new[] { "East", "West" }))
                .RuleFor(u => u.TenantId, (f, u) => this.tenantId)
                .FinishWith((f, u) =>
                {
                    u.SetCustomerNumber();
                    u.SetNormalDelivery();
                    u.SetReturnPeriod(DateTime.UtcNow.AddDays(3));
                });
        }

        [Fact]
        public async Task FindAllAsync_Test()
        {
            // arrange/act
            var result = await this.sut.FindAllAsync().AnyContext();

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task FindAllAsync_WithOrder_Test()
        {
            // arrange/act
            var result = await this.sut.FindAllAsync(
                new FindOptions<Order>(order: new OrderOption<Order>(e => e.Region))).AnyContext();

            // collection indexing should be changed
            // "kind": "Range",
            // "dataType": "String",  <<< while order is based on string field
            // "precision": -1

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
            result.First().Region.ShouldBe("East");
            result.Last().Region.ShouldBe("West");
        }

        [Fact]
        public async Task FindAllAsync_WithOptions_Test()
        {
            // arrange/act
            var result = await this.sut.FindAllAsync(
                new FindOptions<Order>(take: 3)).AnyContext();

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
            result.Count().ShouldBe(3);
        }

        [Fact]
        public async Task FindAllAsync_WithTenantExtension_Test()
        {
            // arrange/act
            var result = await this.sut.FindAllAsync(this.tenantId, default).AnyContext();

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task FindAllAsync_WithSpecification_Test()
        {
            // arrange/act
            var result = await this.sut.FindAllAsync(
                new HasEastRegionOrderSpecification()).AnyContext();

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();

            // arrange/act
            result = await this.sut.FindAllAsync(
                new Specification<Order>(e => e.Region == "East")).AnyContext();

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty(); // fails because of gender enum (=0 instead of Male)
        }

        [Fact]
        public async Task FindAllAsync_WithAndSpecification_Test()
        {
            // arrange/act
            var result = await this.sut.FindAllAsync(
                new HasEastRegionOrderSpecification()
                .And(new Specification<Order>(e => e.Region == "East"))).AnyContext();

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();

            result = await this.sut.FindAllAsync(new[]
            {
                new HasEastRegionOrderSpecification(),
                new Specification<Order>(e => e.Region == "East")
            }).AnyContext();

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task FindAllAsync_WithOrSpecification_Test()
        {
            // arrange/act
            var result = await this.sut.FindAllAsync(
                    new HasEastRegionOrderSpecification()
                    .Or(new Specification<Order>(e => e.Region == "East"))).AnyContext();

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
        }

        [Fact(Skip = "expression mapping issue")]
        public async Task FindAllAsync_WithNotSpecification_Test()
        {
            // arrange/act
            var result = await this.sut.FindAllAsync(
                    new HasEastRegionOrderSpecification()
                    .And(new Specification<Order>(e => e.Region == "East")
                    .Not())).AnyContext(); // NOT is not mapped correctly (EntityMapper > Automapper)

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task FindAllAsync_WithSpecifications_Test()
        {
            // arrange/act
            var result = await this.sut.FindAllAsync(
                new[]
                {
                    new HasEastRegionOrderSpecification(),
                    new Specification<Order>(e => e.Region == "East")
                }).AnyContext();

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task FindOneAsync_Test()
        {
            // arrange
            var entities = await this.sut.FindAllAsync(
                new FindOptions<Order>(take: 1)).AnyContext();

            // act
            var result = await this.sut.FindOneAsync(entities.FirstOrDefault()?.Id).AnyContext();

            // assert
            result.ShouldNotBeNull();
            result.Id.ShouldBe(entities.FirstOrDefault()?.Id);
            result.CustomerNumber.ShouldNotBeNullOrEmpty();
            result.DeliveryPeriod.ShouldNotBeNull();
            result.DeliveryPeriod.StartDate.ShouldNotBeNull();
            result.ReturnPeriod.ShouldNotBeNull();
            result.ReturnPeriod.StartDate.ShouldNotBeNull();
            //result.State.ShouldNotBeNull();
            //result.State.CreatedDescription.ShouldNotBeNull(); // EntityInsertDomainEventHandler
            //result.State.CreatedBy.ShouldNotBeNull(); // EntityInsertDomainEventHandler
        }

        [Fact]
        public async Task FindOneAsync_UnknownId_Test()
        {
            // arrange/act
            var result = await this.sut.FindOneAsync(Guid.NewGuid().ToString()).AnyContext();

            // assert
            result.ShouldBeNull();
        }

        [Fact]
        public async Task InsertAsync_Test()
        {
            // arrange/act
            var result = await this.sut.InsertAsync(this.entityFaker.Generate()).AnyContext();

            // assert
            result.ShouldNotBeNull();
            result.Id.ShouldNotBeNull();
            //result.IdentifierHash.ShouldNotBeNull(); // EntityInsertDomainEventHandler
            //result.State.ShouldNotBeNull();
            //result.State.CreatedDescription.ShouldNotBeNull(); // EntityInsertDomainEventHandler
            //result.State.CreatedBy.ShouldNotBeNull(); // EntityInsertDomainEventHandler
        }

        [Fact]
        public async Task UpsertAsync_Test()
        {
            for (var i = 1; i < 10; i++)
            {
                // arrange/act
                var result = await this.sut.UpsertAsync(this.entityFaker.Generate()).AnyContext();

                // assert
                result.action.ShouldNotBe(RepositoryActionResult.None);
                result.entity.ShouldNotBeNull();
                result.entity.Id.ShouldNotBeNull();
                //result.entity.IdentifierHash.ShouldNotBeNull(); // EntityInsertDomainEventHandler
                //result.entity.State.ShouldNotBeNull();
                //result.entity.State.CreatedDescription.ShouldNotBeNull(); // EntityInsertDomainEventHandler
                //result.entity.State.CreatedBy.ShouldNotBeNull(); // EntityInsertDomainEventHandler
            }
        }

        [Fact]
        public async Task DeleteAsync_ByEntity_Test()
        {
            // arrange
            var entities = await this.sut.FindAllAsync(
                new FindOptions<Order>(take: 1)).AnyContext();

            // act
            var result = await this.sut.DeleteAsync(entities.FirstOrDefault()).AnyContext();

            // assert
            result.ShouldBe(RepositoryActionResult.Deleted);
            (await this.sut.FindOneAsync(entities.FirstOrDefault()?.Id).AnyContext()).ShouldBeNull();
        }

        [Fact]
        public async Task DeleteAsync_ById_Test()
        {
            // arrange
            var entities = await this.sut.FindAllAsync(
                new FindOptions<Order>(take: 1)).AnyContext();
            var id = entities.FirstOrDefault()?.Id;

            // act
            var result = await this.sut.DeleteAsync(id).AnyContext();

            // assert
            result.ShouldBe(RepositoryActionResult.Deleted);
            (await this.sut.FindOneAsync(id).AnyContext()).ShouldBeNull();
        }

        [Fact]
        public async Task DeleteAsync_UnknownId_Test()
        {
            // arrange/act
            var result = await this.sut.DeleteAsync(Guid.NewGuid().ToString()).AnyContext();

            // assert
            result.ShouldBe(RepositoryActionResult.None);
        }
    }
}
