namespace Naos.Sample.IntegrationTests.Catalogs.Infrastructure
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Bogus;
    using Microsoft.Extensions.DependencyInjection;
    using Naos.Foundation;
    using Naos.Foundation.Infrastructure;
    using Naos.Sample.Catalogs.Domain;
    using Shouldly;
    using Xunit;

    public class SqlServerDocumentProviderTests : BaseTest
    {
        private readonly IDocumentProvider<Product> sut;
        private readonly Faker<Product> entityFaker;
        private readonly string tenantId = "naos_sample_test";

        public SqlServerDocumentProviderTests()
        {
            this.sut = this.ServiceProvider.GetService<IDocumentProvider<Product>>();
            this.entityFaker = new Faker<Product>() //https://github.com/bchavez/Bogus
                .RuleFor(u => u.Id, f => Guid.NewGuid().ToString())
                .RuleFor(u => u.CategoryId, f => Guid.NewGuid())
                .RuleFor(u => u.CatalogName, f => "test")
                .RuleFor(u => u.Number, f => f.Random.Replace("??-#####"))
                .RuleFor(u => u.Name, f => f.Commerce.ProductName())
                .RuleFor(u => u.Description, f => f.Lorem.Random.Words(10))
                .RuleFor(u => u.Price, f => f.Commerce.Price(1, 999).To<decimal>())
                .RuleFor(u => u.Region, (f, u) => f.PickRandom(new[] { "East", "West" }))
                .RuleFor(u => u.HasStock, (f, u) => f.PickRandom(new[] { true, false }))
                .RuleFor(u => u.TenantId, f => this.tenantId);
        }

        //[Fact]
        //public async Task ResetAsync_Test()
        //{
        //    await this.sut.ResetAsync().AnyContext();
        //}

        [Fact]
        public async Task UpsertAsync_Test()
        {
            for (int i = 0; i < 10; i++)
            {
                // arrange
                var entity = this.entityFaker.Generate();

                // act
                var result = await this.sut.UpsertAsync(entity.Id, entity).AnyContext();

                // assert
                result.ShouldBe(ProviderAction.Inserted);
            }
        }

        [Fact]
        public async Task LoadValuesAsync_Test()
        {
            // arrange

            // act
            var results = await this.sut.LoadValuesAsync().AnyContext();

            // assert
            results.ShouldNotBeNull();
            results.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task LoadValuesAsync_WithExpression1_Test()
        {
            // arange/act
            var results = await this.sut.LoadValuesAsync(p => p.Region == "East").AnyContext();

            // assert
            results.ShouldNotBeNull();
            results.ShouldNotBeEmpty();
            results.ToList().ForEach(p => p.Region.ShouldBe("East"));
        }

        [Fact]
        public async Task LoadValuesAsync_WithExpression2_Test()
        {
            // arange/act
            var results = await this.sut.LoadValuesAsync(p => p.HasStock).AnyContext();

            // assert
            results.ShouldNotBeNull();
            results.ShouldNotBeEmpty();
            results.ToList().ForEach(p => p.HasStock.ShouldBeTrue());
        }

        [Fact]
        public async Task LoadValuesAsync_WithExpression3_Test()
        {
            // arange/act
            var results = await this.sut.LoadValuesAsync(p => !p.HasStock).AnyContext();

            // assert
            results.ShouldNotBeNull();
            results.ShouldNotBeEmpty();
            results.ToList().ForEach(p => p.HasStock.ShouldBeFalse());
        }

        [Fact]
        public async Task LoadValuesAsync_WithExpression4_Test()
        {
            // arange/act
            var results = await this.sut.LoadValuesAsync(p => p.Region != "Unknown" && p.Region != "West" && p.Region == "East").AnyContext();

            // assert
            results.ShouldNotBeNull();
            results.ShouldNotBeEmpty();
            results.ToList().ForEach(p =>
            {
                p.Region.ShouldBe("East");
            });
        }

        [Fact]
        public async Task LoadValuesAsync_WithExpression5_Test()
        {
            // arange/act
            var results = await this.sut.LoadValuesAsync(p => !p.HasStock && p.Region == "East").AnyContext();

            // assert
            results.ShouldNotBeNull();
            results.ShouldNotBeEmpty();
            results.ToList().ForEach(p =>
            {
                p.HasStock.ShouldBeFalse();
                p.Region.ShouldBe("East");
            });
        }

        [Fact]
        public async Task LoadValuesAsync_WithExpression6_Test()
        {
            // arange/act
            var results = await this.sut.LoadValuesAsync(p => p.Price > 0).AnyContext();

            // assert
            results.ShouldNotBeNull();
            results.ShouldNotBeEmpty();
            results.ToList().ForEach(p => p.Price.ShouldBeGreaterThan(0));
        }
    }
}
