namespace Naos.Sample.IntegrationTests.Customers.Domain
{
    using System.Linq;
    using System.Threading.Tasks;
    using Bogus;
    using Microsoft.Extensions.DependencyInjection;
    using Naos.Core.Domain.Repositories;
    using Naos.Core.Domain.Specifications;
    using Naos.Sample.Customers.Domain;
    using Shouldly;
    using Xunit;

    public class CustomerRepositoryTests : BaseTest
    {
        // https://xunit.github.io/docs/shared-context.html
        private readonly ICustomerRepository sut;
        private readonly Faker<Customer> entityFaker;
        private readonly string tenantId = "naos_sample_test";

        public CustomerRepositoryTests()
        {
            this.sut = this.ServiceProvider.GetService<ICustomerRepository>();
            this.entityFaker = new Faker<Customer>() //https://github.com/bchavez/Bogus
                .RuleFor(u => u.CustomerNumber, f => f.Random.Replace("??-#####"))
                .RuleFor(u => u.Gender, f => f.PickRandom(new[] { "Male", "Female" }))
                .RuleFor(u => u.FirstName, (f, u) => f.Name.FirstName())
                .RuleFor(u => u.LastName, (f, u) => f.Name.LastName())
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
                .RuleFor(u => u.Region, (f, u) => f.PickRandom(new[] { "East", "West" }))
                .RuleFor(u => u.TenantId, (f, u) => this.tenantId);
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
        public async Task FindAllAsync_WithOrder_Test()
        {
            // arrange/act
            var result = await this.sut.FindAllAsync(
                new FindOptions<Customer>(order: new OrderOption<Customer>(e => e.Region))).ConfigureAwait(false);

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
                new FindOptions<Customer>(take: 3)).ConfigureAwait(false);

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
            result.Count().ShouldBe(3);
        }

        [Fact]
        public async Task FindAllAsync_WithTenantExtension_Test()
        {
            // arrange/act
            var result = await this.sut.FindAllAsync(this.tenantId, default).ConfigureAwait(false);

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task FindAllAsync_WithSpecification_Test()
        {
            // arrange/act
            var result = await this.sut.FindAllAsync(
                new HasEastRegionSpecification()).ConfigureAwait(false);

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();

            // arrange/act
            result = await this.sut.FindAllAsync(
                new Specification<Customer>(e => e.Gender == "Male")).ConfigureAwait(false);

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty(); // fails because of gender enum (=0 instead of Male)
        }

        [Fact]
        public async Task FindAllAsync_WithAndSpecification_Test()
        {
            // arrange/act
            var result = await this.sut.FindAllAsync(
                    new HasEastRegionSpecification()
                    .And(new Specification<Customer>(e => e.Gender == "Male"))).ConfigureAwait(false);

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task FindAllAsync_WithOrSpecification_Test()
        {
            // arrange/act
            var result = await this.sut.FindAllAsync(
                    new HasEastRegionSpecification()
                    .Or(new Specification<Customer>(e => e.Gender == "Male"))).ConfigureAwait(false);

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task FindAllAsync_WithNotSpecification_Test()
        {
            // arrange/act
            var result = await this.sut.FindAllAsync(
                    new HasEastRegionSpecification()
                    .And(new Specification<Customer>(e => e.Gender == "Male")
                    .Not())).ConfigureAwait(false);

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
                    new HasEastRegionSpecification(),
                    new Specification<Customer>(e => e.Gender == "Male")
                }).ConfigureAwait(false);

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task FindOneAsync_Test()
        {
            // arrange
            var entities = await this.sut.FindAllAsync(
                new FindOptions<Customer>(take: 1)).ConfigureAwait(false);

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
            var result = await this.sut.InsertAsync(this.entityFaker.Generate()).ConfigureAwait(false);

            // assert
            result.ShouldNotBeNull();
            result.Id.ShouldNotBeNull();
            result.IdentifierHash.ShouldNotBeNull(); // EntityInsertDomainEventHandler
            result.State.ShouldNotBeNull();
            result.State.CreatedDescription.ShouldNotBeNull(); // EntityInsertDomainEventHandler
            result.State.CreatedBy.ShouldNotBeNull(); // EntityInsertDomainEventHandler
        }

        [Fact]
        public async Task UpsertAsync_Test()
        {
            for (int i = 1; i < 10; i++)
            {
                // arrange/act
                var result = await this.sut.UpsertAsync(this.entityFaker.Generate()).ConfigureAwait(false);

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
                new FindOptions<Customer>(take: 1)).ConfigureAwait(false);

            // act
            await this.sut.DeleteAsync(entities.FirstOrDefault()).ConfigureAwait(false);
            var result = await this.sut.FindOneAsync(entities.FirstOrDefault()?.Id).ConfigureAwait(false);

            // assert
            result.ShouldBeNull();
        }
    }
}
