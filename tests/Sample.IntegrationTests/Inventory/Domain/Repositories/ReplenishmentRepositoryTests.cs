namespace Naos.Sample.IntegrationTests.Inventory.Domain
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Bogus;
    using Microsoft.Extensions.DependencyInjection;
    using Naos.Foundation;
    using Naos.Foundation.Domain;
    using Naos.Sample.Inventory.Domain;
    using Shouldly;
    using Xunit;

    public class ReplenishmentRepositoryTests : BaseTest
    {
        // https://xunit.github.io/docs/shared-context.html
        private readonly IGenericRepository<ProductReplenishment> sut;
        private readonly Faker<ProductReplenishment> entityFaker;
        private readonly string tenantId = "naos_sample_test";

        public ReplenishmentRepositoryTests()
        {
            //this.sut = this.ServiceProvider.GetService<IUserAccountRepository>();
            this.sut = this.ServiceProvider.GetRequiredService<IReplenishmentRepository>();
            var domains = new[] { "East", "West" };
            this.entityFaker = new Faker<ProductReplenishment>() //https://github.com/bchavez/Bogus
                //.RuleFor(u => u.Id, f => Guid.NewGuid().ToString())
                .RuleFor(u => u.Number, f => f.Random.Replace("??-#####"))
                .RuleFor(u => u.Region, (f, u) => f.PickRandom(new[] { "East", "West" }))
                .RuleFor(u => u.Quantity, f => f.Random.Int(0, 999))
                .RuleFor(u => u.ShippedFromLocation, (f, u) => f.PickRandom(new[] { "de", "us", "ch", "pl" }))
                .RuleFor(u => u.ArrivedAtLocation, (f, u) => f.PickRandom(new[] { "de", "us", "ch", "pl" }))
                .RuleFor(u => u.ShippedDate, (f, u) => DateTime.UtcNow)
                .RuleFor(u => u.ArrivedDate, (f, u) => DateTime.UtcNow)
                .RuleFor(u => u.TenantId, f => this.tenantId);
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
                new FindOptions<ProductReplenishment>(order: new OrderOption<ProductReplenishment>(e => e.Region))).AnyContext();

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
            result.FirstOrDefault()?.Region.ShouldBe("East");
            result.LastOrDefault()?.Region.ShouldBe("West");
        }

        [Fact]
        public async Task FindAllAsync_WithOptions_Test()
        {
            // arrange/act
            var result = await this.sut.FindAllAsync(
                new FindOptions<ProductReplenishment>(take: 3)).AnyContext();

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
            result.Count().ShouldBe(3);
        }

        [Fact(Skip = "unresolved expression mapping issue")]
        public async Task FindAllAsync_WithTenantExtension_Test()
        {
            // DOES NOT WORK DUE TO MAppING > Convert({document}, ITenantEntity).TenantId is not supported.
            // arrange/act
            var result = await this.sut.FindAllAsync(this.tenantId, default).AnyContext();

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task FindAllAsync_WithTenantSpecification_Test()
        {
            // arrange/act
            var result = await this.sut.FindAllAsync(
                new Specification<ProductReplenishment>(e => e.TenantId == this.tenantId), default).AnyContext();

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task FindAllAsync_WithSpecification_Test()
        {
            // arrange/act
            var result = await this.sut.FindAllAsync(
                new ReplenishmentHasRegionSpecification("East")).AnyContext();

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();

            // arrange/act
            result = await this.sut.FindAllAsync(
                new Specification<ProductReplenishment>(e => e.Quantity > 0)).AnyContext();

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task FindAllAsync_WithAndSpecification1_Test()
        {
            // AND Specification is giving problems with expression translation in mongo.driver

            // arrange/act
            var result = await this.sut.FindAllAsync(
                new ReplenishmentHasRegionSpecification("East")
                .And(new Specification<ProductReplenishment>(e => e.Quantity > 0))).AnyContext();

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task FindAllAsync_WithMultipleSpecifications_Test()
        {
            // arrange/act
            var result = await this.sut.FindAllAsync(
                new[]
                {
                    new ReplenishmentHasRegionSpecification("East"),
                    new Specification<ProductReplenishment>(e => e.Quantity > 0)
                }).AnyContext();

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task FindAllAsync_WithMultiSpecification_Test()
        {
            // arrange/act
            var result = await this.sut.FindAllAsync(
                new[]
                {
                    new Specification<ProductReplenishment>(e => e.Region == "East" && e.Quantity > 0 )
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
                new FindOptions<ProductReplenishment>(take: 1)).AnyContext();

            // act
            var entity = await this.sut.FindOneAsync(entities.FirstOrDefault()?.Id).AnyContext();

            // assert
            entity.ShouldNotBeNull();
            entity.Region.ShouldNotBeNull();
            entity.Number.ShouldNotBeNull();
        }

        [Fact]
        public async Task FindOneAndUpdateAsync_Test()
        {
            // arrange
            var entities = await this.sut.FindAllAsync(
                new FindOptions<ProductReplenishment>(take: 1)).AnyContext();

            // act
            var entity = await this.sut.FindOneAsync(entities.FirstOrDefault()?.Id).AnyContext();
            entity.ShouldNotBeNull();

            var newNumber = RandomGenerator.GenerateString(8);
            entity.Number = newNumber;
            await this.sut.UpsertAsync(entity).AnyContext();
            var modifiedEntity = await this.sut.FindOneAsync(entity.Id).AnyContext();

            // assert
            entity.ShouldNotBeNull();
            modifiedEntity.ShouldNotBeNull();
            modifiedEntity.Number.ShouldBe(newNumber);
        }

        [Fact]
        public async Task FindOneAndDeleteAsync_Test() // SoftDelete because of decorator
        {
            // arrange
            var entities = await this.sut.FindAllAsync(
                new FindOptions<ProductReplenishment>(take: 1)).AnyContext();

            // act
            var entity = await this.sut.FindOneAsync(entities.FirstOrDefault()?.Id).AnyContext();
            entity.ShouldNotBeNull();
            var result = await this.sut.DeleteAsync(entity).AnyContext();

            // assert
            result.ShouldBe(ActionResult.Deleted);
            (await this.sut.FindOneAsync(entities.FirstOrDefault()?.Id).AnyContext()).ShouldBeNull();
        }

        [Fact]
        public async Task InsertAsync_Test()
        {
            // arrange/act
            var result = await this.sut.InsertAsync(this.entityFaker.Generate()).AnyContext();

            // assert
            result.ShouldNotBeNull();
            result.Id.ShouldNotBeNull();
        }

        [Fact]
        public async Task UpsertAsync_Test()
        {
            for (var i = 1; i < 10; i++)
            {
                // arrange/act
                var result = await this.sut.UpsertAsync(this.entityFaker.Generate()).AnyContext();

                // assert
                result.action.ShouldNotBe(ActionResult.None);
                result.entity.ShouldNotBeNull();
                result.entity.Id.ShouldNotBeNull();
                //result.entity.IdentifierHash.ShouldNotBeNull(); // EntityInsertDomainEventHandler
                //result.entity.State.ShouldNotBeNull();
                //result.entity.State.CreatedDescription.ShouldNotBeNull(); // EntityInsertDomainEventHandler
                //result.entity.State.CreatedBy.ShouldNotBeNull(); // EntityInsertDomainEventHandler
            }
        }

        [Fact]
        public async Task DeleteAsync_Test()
        {
            // arrange
            var entities = await this.sut.FindAllAsync(
                new FindOptions<ProductReplenishment>(take: 1)).AnyContext();

            // act
            await this.sut.DeleteAsync(entities.FirstOrDefault()).AnyContext();
            var result = await this.sut.FindOneAsync(entities.FirstOrDefault()?.Id).AnyContext();

            // assert
            result.ShouldBeNull();
        }
    }
}
