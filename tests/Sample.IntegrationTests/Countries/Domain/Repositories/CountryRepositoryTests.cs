namespace Naos.Sample.IntegrationTests.Countries.Domain
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Naos.Core.Domain.Repositories;
    using Naos.Core.Domain.Specifications;
    using Naos.Sample.Countries.Domain;
    using Shouldly;
    using Xunit;

    public class CountryRepositoryTests : BaseTest
    {
        // https://xunit.github.io/docs/shared-context.html
        private readonly ICountryRepository sut;
        private readonly string tenantId = "naos_sample_test";

        public CountryRepositoryTests()
        {
            this.sut = this.ServiceProvider.GetService<ICountryRepository>();
        }

        [Fact]
        public async Task FindAllAsync_Test()
        {
            // arrange/act
            var result = await this.sut.FindAllAsync().ConfigureAwait(false);

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task FindAllAsync_WithOptions_Test()
        {
            // arrange/act
            var result = await this.sut.FindAllAsync(
                new FindOptions<Country>(take: 2)).ConfigureAwait(false);

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
            result.Count().ShouldBe(2);
        }

        //[Fact]
        //public async Task FindAllAsync_WithTenantExtension_Test()
        //{
        //    // causes issues when used in mapped repos <TEntity, TDesitination>, Unable to cast object of type 'xxxDto' to type 'Naos.Core.Domain.ITenantEntity'. better use the tenant decorator for this
        //    // arrange/act
        //    var result = await this.sut.FindAllAsync(this.tenantId, default).ConfigureAwait(false);

        //    // assert
        //    result.ShouldNotBeNull();
        //    result.ShouldNotBeEmpty();
        //}

        [Fact]
        public async Task FindAllAsync_WithSpecification_Test()
        {
            // arrange/act
            var result = await this.sut.FindAllAsync(
                new HasNameSpecification("Germany")).ConfigureAwait(false);

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
            result.Count().ShouldBe(1);
            result.First().Name.ShouldBe("Germany");
        }

        [Fact]
        public async Task FindAllAsync_WithAndSpecification_Test()
        {
            // arrange/act
            var result = await this.sut.FindAllAsync(
                    new HasNameSpecification("Germany")
                    .And(new HasCodeSpecification("de"))).ConfigureAwait(false);

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
            result.Count().ShouldBe(1);
            result.First().Name.ShouldBe("Germany");
        }

        [Fact]
        public async Task FindAllAsync_WithOrSpecification_Test()
        {
            // arrange/act
            var result = await this.sut.FindAllAsync(
                    new HasNameSpecification("Germany")
                    .Or(new HasCodeSpecification("nl"))).ConfigureAwait(false);

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
            result.Count().ShouldBe(2);
        }

        [Fact]
        public async Task FindAllAsync_WithNotSpecification_Test()
        {
            // arrange/act
            var result = await this.sut.FindAllAsync(
                    new HasNameSpecification("Germany")
                    .And(new HasCodeSpecification("nl")
                    .Not())).ConfigureAwait(false);

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
            result.Count().ShouldBe(1);
            result.First().Name.ShouldBe("Germany");
        }

        [Fact]
        public async Task FindAllAsync_WithSpecifications_Test()
        {
            // arrange/act
            var result = await this.sut.FindAllAsync(
                new List<ISpecification<Country>>
                {
                    new HasNameSpecification("Germany"),
                    new HasCodeSpecification("de")
                }).ConfigureAwait(false);

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
            result.Count().ShouldBe(1);
            result.First().Name.ShouldBe("Germany");
        }

        [Fact]
        public async Task FindOneAsync_Test()
        {
            // arrange
            var entities = await this.sut.FindAllAsync(
                new FindOptions<Country>(take: 1)).ConfigureAwait(false);

            // act
            var result = await this.sut.FindOneAsync(entities.FirstOrDefault()?.Id).ConfigureAwait(false);

            // assert
            result.ShouldNotBeNull();
            result.Id.ShouldBe(entities.FirstOrDefault()?.Id);
        }

        [Fact]
        public async Task InsertAsync_Test()
        {
            // arrange/act
            var result = await this.sut.InsertAsync(
                new Country { Code = "fr", LanguageCodes = new[] { "fr-fr" }, Name = "France", TenantId = this.tenantId, Id = "fr" }).ConfigureAwait(false);

            // assert
            result.ShouldNotBeNull();
            result.Id.ShouldNotBeNull();
            result.IdentifierHash.ShouldNotBeNull(); // EntityInsertDomainEventHandler
            result.State.ShouldNotBeNull();
            result.State.CreatedDescription.ShouldNotBeNull(); // EntityInsertDomainEventHandler
            result.State.CreatedBy.ShouldNotBeNull(); // EntityInsertDomainEventHandler

            using (var scope = this.ServiceProvider.CreateScope())
            {
                var sut2 = scope.ServiceProvider.GetService<ICountryRepository>();
                var entity = await sut2.FindOneAsync("fr").ConfigureAwait(false);

                entity.ShouldNotBeNull();
                entity.Id.ShouldBe("fr");
            }
        }

        //[Fact]
        //public async Task UpsertAsync_Test()
        //{
        //    for (int i = 1; i < 10; i++)
        //    {
        //        // arrange/act
        //        var result = await this.sut.UpsertAsync(this.entityFaker.Generate()).ConfigureAwait(false);

        //        // assert
        //        result.action.ShouldNotBe(UpsertAction.None);
        //        result.entity.ShouldNotBeNull();
        //        result.entity.Id.ShouldNotBeNull();
        //    }
        //}

        //[Fact]
        //public async Task DeleteAsync_Test()
        //{
        //    // arrange
        //    var entities = await this.sut.FindAllAsync(
        //        new FindOptions<Customer>(take: 1)).ConfigureAwait(false);

        //    // act
        //    await this.sut.DeleteAsync(entities.FirstOrDefault()).ConfigureAwait(false);
        //    var result = await this.sut.FindOneAsync(entities.FirstOrDefault()?.Id).ConfigureAwait(false);

        //    // assert
        //    result.ShouldBeNull();
        //}
    }
}
