namespace Naos.Sample.IntegrationTests.Catalogs.Infrastructure
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Bogus;
    using Microsoft.Extensions.DependencyInjection;
    using Naos.Foundation;
    using Naos.Foundation.Domain;
    using Naos.Foundation.Infrastructure;
    using Naos.Sample.Catalogs.Domain;
    using Shouldly;
    using Xunit;

    public class SqlServerDocumentRepositoryTests : BaseTest
    {
        private readonly IProductRepository sut;
        private readonly Faker<Product> entityFaker;
        private readonly string tenantId = "naos_sample_test";

        public SqlServerDocumentRepositoryTests()
        {
            this.sut = this.ServiceProvider.GetRequiredService<IProductRepository>();
            this.entityFaker = new Faker<Product>() //https://github.com/bchavez/Bogus
                .RuleFor(u => u.Id, f => Guid.NewGuid().ToString())
                .RuleFor(u => u.CategoryId, f => Guid.NewGuid())
                .RuleFor(u => u.CatalogName, f => "test")
                .RuleFor(u => u.Number, f => f.Random.Replace("??-#####"))
                .RuleFor(u => u.Name, f => f.Commerce.ProductName())
                .RuleFor(u => u.Description, f => f.Lorem.Random.Words(10))
                .RuleFor(u => u.Type, f => f.PickRandom(new[] { "product", "subscription" }))
                .RuleFor(u => u.Price, f => f.Commerce.Price(1, 999).To<decimal>())
                .RuleFor(u => u.Region, (f, u) => f.PickRandom(new[] { "East", "West" }))
                .RuleFor(u => u.HasStock, (f, u) => f.PickRandom(new[] { true, false }))
                .RuleFor(u => u.TenantId, f => this.tenantId);
        }

        [Fact]
        public async Task UpsertAsync_Test()
        {
            for (var i = 0; i < 10; i++)
            {
                // arrange
                var entity = this.entityFaker.Generate();

                // act
                var result = await this.sut.UpsertAsync(entity).AnyContext();

                // assert
                result.entity.ShouldNotBeNull();
                result.action.ShouldBe(ActionResult.Inserted);
            }
        }

        [Fact]
        public async Task ExistsAsync_Test()
        {
            // arrange
            var entity = this.entityFaker.Generate();
            await this.sut.UpsertAsync(entity).AnyContext();

            // act
            var result = await this.sut.ExistsAsync(entity.Id).AnyContext();

            // assert
            result.ShouldBeTrue();
        }

        [Fact]
        public void FindAllAsync_All_Test()
        {
            // arrange/act
            var results = this.sut.FindAllAsync();

            // assert
            results.ShouldNotBeNull();
            //results.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task FindAllAsync_WithOptions_Test()
        {
            // arrange/act
            var results = await this.sut.FindAllAsync(new FindOptions<Product>(skip: 5, take: 2)).AnyContext();

            // assert
            results.Count().ShouldBe(2);
        }

        [Fact]
        public async Task FindOneAsync_Test()
        {
            // arrange
            var entity = this.entityFaker.Generate();
            await this.sut.UpsertAsync(entity).AnyContext();

            // act
            var result = await this.sut.FindOneAsync(entity.Id).AnyContext();

            // assert
            result.ShouldNotBeNull();
            result.Id.ShouldBe(entity.Id);
        }

        [Fact]
        public async Task FindAllAsync_WithSpecification_Test()
        {
            // arange/act
            var results = await this.sut.FindAllAsync(new Specification<Product>(e => e.Region == "East")).AnyContext();

            // assert
            results.ShouldNotBeNull();
            results.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task FindAllAsync_WithNonIndexedSpecification_Test()
        {
            // arange/act
            var results = await this.sut.FindAllAsync(new Specification<Product>(e => e.Type == "product")).AnyContext();

            // assert
            results.ShouldNotBeNull();
            results.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task FindAllAsync_WithSpecification2_Test()
        {
            // arange/act
            var results = await this.sut.FindAllAsync(new Specification<Product>(e => e.HasStock)).AnyContext();

            // assert
            results.ShouldNotBeNull();
            results.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task FindAllAsync_WithSpecification3_Test()
        {
            // arange/act
            var results = await this.sut.FindAllAsync(new Specification<Product>(e => !e.HasStock)).AnyContext();

            // assert
            results.ShouldNotBeNull();
            results.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task FindAllAsync_WithSpecification4_Test()
        {
            // arange/act
            var results = await this.sut.FindAllAsync(new Specification<Product>(e => e.Region != "Unknown" && e.Region != "West" && e.Region == "East")).AnyContext();

            // assert
            results.ShouldNotBeNull();
            results.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task FindAllAsync_WithSpecification5_Test()
        {
            // arange/act
            var results = await this.sut.FindAllAsync(new Specification<Product>(e => !e.HasStock && e.Region == "East")).AnyContext();

            // assert
            results.ShouldNotBeNull();
            results.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task FindAllAsync_WithSpecification6_Test()
        {
            // arange/act
            var results = await this.sut.FindAllAsync(new Specification<Product>(e => e.Price > 0)).AnyContext();

            // assert
            results.ShouldNotBeNull();
            results.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task FindAllAsync_WithSpecification7_Test()
        {
            // arange/act
#pragma warning disable CA1307 // Specify StringComparison
            var results = await this.sut.FindAllAsync(new Specification<Product>(e => e.Region.Contains("ast"))).AnyContext();

            // assert
            results.ShouldNotBeNull();
            results.ShouldNotBeEmpty();
        }
    }
}
