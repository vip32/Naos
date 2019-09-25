namespace Naos.Sample.IntegrationTests.Inventory.Domain
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Bogus;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Naos.Foundation;
    using Naos.Foundation.Domain;
    using Naos.Sample.Inventory.Domain;
    using Shouldly;
    using Xunit;

    public class InventoryRepositoryTests : BaseTest
    {
        // https://xunit.github.io/docs/shared-context.html
        private readonly IGenericRepository<ProductInventory> sut;
        private readonly Faker<ProductInventory> entityFaker;
        private readonly string tenantId = "naos_sample_test";

        public InventoryRepositoryTests()
        {
            //this.sut = this.ServiceProvider.GetService<IUserAccountRepository>();
            this.sut = this.ServiceProvider.GetRequiredService<IInventoryRepository>();
            var domains = new[] { "East", "West" };
            this.entityFaker = new Faker<ProductInventory>() //https://github.com/bchavez/Bogus
                //.RuleFor(u => u.Id, f => Guid.NewGuid().ToString())
                .RuleFor(u => u.Number, f => f.Random.Replace("??-#####"))
                .RuleFor(u => u.Region, (f, u) => f.PickRandom(new[] { "East", "West" }))
                .RuleFor(u => u.Quantity, f => f.Random.Int(0, 999))
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
                new FindOptions<ProductInventory>(order: new OrderOption<ProductInventory>(e => e.Region))).AnyContext();

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
                new FindOptions<ProductInventory>(take: 3)).AnyContext();

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
                new HasRegionSpecification("East")).AnyContext();

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();

            // arrange/act
            result = await this.sut.FindAllAsync(
                new Specification<ProductInventory>(e => e.Quantity > 0)).AnyContext();

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task FindAllAsync_WithAndSpecification1_Test()
        {
            // arrange/act
            var result = await this.sut.FindAllAsync(
                new HasRegionSpecification("East")
                .And(new Specification<ProductInventory>(e => e.Quantity > 0))).AnyContext();

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
                    new HasRegionSpecification("East"),
                    new Specification<ProductInventory>(e => e.Quantity > 0)
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
                    new Specification<ProductInventory>(e => e.Region == "East" && e.Quantity > 0 )
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
                new FindOptions<ProductInventory>(take: 1)).AnyContext();

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
                new FindOptions<ProductInventory>(take: 1)).AnyContext();

            // act
            var entity = await this.sut.FindOneAsync(entities.FirstOrDefault()?.Id).AnyContext();
            entity.ShouldNotBeNull();
            entity.State.ShouldNotBeNull();
            entity.Number.ShouldNotBeNull();

            var newNumber = RandomGenerator.GenerateString(8);
            entity.Number = newNumber;
            entity.State.SetUpdated("test", "reason " + new DateTimeEpoch().Epoch);
            await this.sut.UpsertAsync(entity).AnyContext();
            var modifiedEntity = await this.sut.FindOneAsync(entity.Id).AnyContext();

            // assert
            entity.ShouldNotBeNull();
            modifiedEntity.ShouldNotBeNull();
            modifiedEntity.Number.ShouldBe(newNumber);
            modifiedEntity.State.UpdatedReasons.ToString(";").ShouldContain("reason ");
        }

        [Fact]
        public async Task FindOneAndDeleteAsync_Test() // SoftDelete because of decorator
        {
            // arrange
            var entities = await this.sut.FindAllAsync(
                new FindOptions<ProductInventory>(take: 1)).AnyContext();

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
                result.entity.IdentifierHash.ShouldNotBeNull(); // EntityInsertDomainEventHandler
                result.entity.State.ShouldNotBeNull();
                result.entity.State.CreatedDescription.ShouldNotBeNull(); // EntityInsertDomainEventHandler
                result.entity.State.CreatedBy.ShouldNotBeNull(); // EntityInsertDomainEventHandler
            }
        }

        [Fact]
        public async Task DeleteAsync_Test()
        {
            // arrange
            var entities = await this.sut.FindAllAsync(
                new FindOptions<ProductInventory>(take: 1)).AnyContext();

            // act
            await this.sut.DeleteAsync(entities.FirstOrDefault()).AnyContext();
            var result = await this.sut.FindOneAsync(entities.FirstOrDefault()?.Id).AnyContext();

            // assert
            result.ShouldBeNull();
        }
    }
}
