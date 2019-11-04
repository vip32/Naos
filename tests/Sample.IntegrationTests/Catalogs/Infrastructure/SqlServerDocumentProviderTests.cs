namespace Naos.Sample.IntegrationTests.Catalogs.Infrastructure
{
    using System;
    using System.Collections.Generic;
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
                .RuleFor(u => u.Type, f => f.PickRandom(new[] { "product", "subscription" }))
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
            for (var i = 0; i < 10; i++)
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
        public async Task ExistsAsync_Test()
        {
            // arrange
            var entity = this.entityFaker.Generate();
            await this.sut.UpsertAsync(entity.Id, entity).AnyContext();

            // act
            var result = await this.sut.ExistsAsync(entity.Id).AnyContext();

            // assert
            result.ShouldBeTrue();
        }

        [Fact]
        public async Task CountAsync_Test()
        {
            // arrange
            var entity = this.entityFaker.Generate();
            await this.sut.UpsertAsync(entity.Id, entity).AnyContext();

            // act
            var result = await this.sut.CountAsync().AnyContext();

            // assert
            result.ShouldBeGreaterThan(0);
        }

        [Fact]
        public void LoadValuesAsync_All_Test()
        {
            // arrange/act
            var results = this.sut.LoadValuesAsync();

            // assert
            results.ShouldNotBeNull();
            //results.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task LoadValuesAsync_Paged_Test()
        {
            // arrange/act
            var results = this.sut.LoadValuesAsync(skip: 5, take: 2);

            // assert
            var count = 0;
            results.ShouldNotBeNull();
            await foreach(var val in results)
            {
                count++;
            }

            count.ShouldBe(2);
        }

        [Fact]
        public async Task LoadValuesAsync_ByKey_Test()
        {
            // arrange
            var entity = this.entityFaker.Generate();
            await this.sut.UpsertAsync(entity.Id, entity).AnyContext();

            // act
            var results = this.sut.LoadValuesAsync(entity.Id);

            // assert
            var count = 0;
            results.ShouldNotBeNull();
            await foreach (var val in results)
            {
                count++;
                val.Id.ShouldBe(entity.Id);
            }

            count.ShouldBe(1);
        }

        [Fact]
        public async Task LoadValuesAsync_ByTags_Test()
        {
            // arrange
            var entity1 = this.entityFaker.Generate();
            entity1.Region = "en-us";
            var entity2 = this.entityFaker.Generate();
            entity2.Id = entity1.Id; // same ids
            entity2.Region = "de-de";
            await this.sut.UpsertAsync(entity1.Id, entity1, new[] { entity1.Region }).AnyContext();
            await this.sut.UpsertAsync(entity2.Id, entity2, new[] { entity2.Region }).AnyContext();

            // act
            var results = this.sut.LoadValuesAsync(entity1.Id, null, new[] { "de-de" });

            // assert
            var count = 0;
            results.ShouldNotBeNull();
            await foreach (var val in results)
            {
                count++;
                val.Id.ShouldBe(entity1.Id);
                val.Region.ShouldBe("de-de");
            }

            count.ShouldBe(1);
        }

        [Fact]
        public async Task LoadValuesAsync_WithExpression1_Test()
        {
            // arange/act
            var results = this.sut.LoadValuesAsync(p => p.Region == "East");

            // assert
            var count = 0;
            results.ShouldNotBeNull();
            await foreach (var val in results)
            {
                count++;
                val.Region.ShouldBe("East");
            }

            count.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task LoadValuesAsync_WithNonIndexedExpression_Test()
        {
            // arange/act
            var results = this.sut.LoadValuesAsync(p => p.Type == "product");

            // assert
            var count = 0;
            results.ShouldNotBeNull();
            await foreach (var val in results)
            {
                count++;
                //val.Type.ShouldBe("product");
            }

            count.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task LoadValuesAsync_WithExpression2_Test()
        {
            // arange/act
            var results = this.sut.LoadValuesAsync(p => p.HasStock);

            // assert
            results.ShouldNotBeNull();
            var count = 0;
            results.ShouldNotBeNull();
            await foreach (var val in results)
            {
                count++;
                val.HasStock.ShouldBeTrue();
            }

            count.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task LoadValuesAsync_WithExpression3_Test()
        {
            // arange/act
            var results = this.sut.LoadValuesAsync(p => !p.HasStock);

            // assert
            results.ShouldNotBeNull();
            var count = 0;
            results.ShouldNotBeNull();
            await foreach (var val in results)
            {
                count++;
                val.HasStock.ShouldBeFalse();
            }

            count.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task LoadValuesAsync_WithExpression4_Test()
        {
            // arange/act
            var results = this.sut.LoadValuesAsync(p => p.Region != "Unknown" && p.Region != "West" && p.Region == "East");

            // assert
            results.ShouldNotBeNull();
            var count = 0;
            results.ShouldNotBeNull();
            await foreach (var val in results)
            {
                count++;
                val.Region.ShouldBe("East");
            }

            count.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task LoadValuesAsync_WithExpression5_Test()
        {
            // arange/act
            var results = this.sut.LoadValuesAsync(p => !p.HasStock && p.Region == "East");

            // assert
            results.ShouldNotBeNull();
            var count = 0;
            results.ShouldNotBeNull();
            await foreach (var val in results)
            {
                count++;
                val.HasStock.ShouldBeFalse();
                val.Region.ShouldBe("East");
            }

            count.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task LoadValuesAsync_WithExpression6_Test()
        {
            // arange/act
            var results = this.sut.LoadValuesAsync(p => p.Price > 0);

            // assert
            results.ShouldNotBeNull();
            var count = 0;
            results.ShouldNotBeNull();
            await foreach (var val in results)
            {
                count++;
                val.Price.ShouldBeGreaterThan(0);
            }

            count.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task LoadValuesAsync_WithExpression7_Test()
        {
            // arange/act
#pragma warning disable CA1307 // Specify StringComparison
            var results = this.sut.LoadValuesAsync(p => p.Region.Contains("ast"));

            // assert
            results.ShouldNotBeNull();
            var count = 0;
            results.ShouldNotBeNull();
            await foreach (var val in results)
            {
                count++;
                val.Region.ShouldBe("East");
            }

            count.ShouldBeGreaterThan(0);
        }
    }
}
