namespace Naos.Sample.IntegrationTests.UserAccounts.Domain
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Bogus;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Naos.Foundation;
    using Naos.Foundation.Domain;
    using Naos.Sample.UserAccounts.Domain;
    using Naos.Sample.UserAccounts.Infrastructure;
    using Shouldly;
    using Xunit;

    public class UserVisitRepositoryTests : BaseTest
    {
        // https://xunit.github.io/docs/shared-context.html
        private readonly IGenericRepository<UserVisit> sut;
        private readonly Faker<UserVisit> entityFaker;
        private readonly string tenantId = "naos_sample_test";

        public UserVisitRepositoryTests()
        {
            //this.sut = this.ServiceProvider.GetService<IUserAccountRepository>();
            this.sut = this.ServiceProvider.GetRequiredService<IGenericRepository<UserVisit>>();
            this.ServiceProvider.GetRequiredService<UserAccountsDbContext>().Database.Migrate();
            var domains = new[] { "East", "West" };
            this.entityFaker = new Faker<UserVisit>() //https://github.com/bchavez/Bogus
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email())
                .RuleFor(u => u.Timestamp, (f, u) => DateTime.UtcNow)
                .RuleFor(u => u.Region, (f, u) => f.PickRandom(new[] { "East", "West" }))
                .RuleFor(u => u.TenantId, (f, u) => this.tenantId);
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
                new FindOptions<UserVisit>(order: new OrderOption<UserVisit>(e => e.Region))).AnyContext();

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
            result.FirstOrDefault()?.ShouldNotBeNull();
            result.FirstOrDefault()?.Region.ShouldBe("East");
            result.LastOrDefault()?.ShouldNotBeNull();
            result.LastOrDefault()?.Region.ShouldBe("West");
        }

        //[Fact]
        //public async Task FindAllAsync_WithOptions_Test()
        //{
        //    // arrange/act
        //    var result = await this.sut.FindAllAsync(
        //        new FindOptions<UserVisit>(take: 3)).AnyContext();

        //    // assert
        //    result.ShouldNotBeNull();
        //    result.ShouldNotBeEmpty();
        //    result.Count().ShouldBe(3);
        //}

        //[Fact]
        //public async Task FindAllAsync_WithTenantExtension_Test()
        //{
        //    // arrange/act
        //    var result = await this.sut.FindAllAsync(this.tenantId, default).AnyContext();

        //    // assert
        //    result.ShouldNotBeNull();
        //    result.ShouldNotBeEmpty();
        //}

        [Fact]
        public async Task FindAllAsync_WithSpecification_Test()
        {
            // arrange/act
            var result = await this.sut.FindAllAsync(
                new HasDomainUserVisitSpecification("East")).AnyContext();

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();

            // arrange/act
            result = await this.sut.FindAllAsync(
                new Specification<UserVisit>(e => e.Region == "East")).AnyContext();

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty(); // fails because of gender enum (=0 instead of Male)
        }

        [Fact]
        public async Task FindAllAsync_WithAndSpecification_Test()
        {
            // arrange/act
            var result = await this.sut.FindAllAsync(
                new HasDomainUserVisitSpecification("East")
                .And(new Specification<UserVisit>(e => e.Region == "East"))).AnyContext();

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
        }

        //[Fact]
        //public async Task FindAllAsync_WithOrSpecification_Test()
        //{
        //    // arrange/act
        //    var result = await this.sut.FindAllAsync(
        //            new HasEastRegionSpecification()
        //            .Or(new Specification<UserVisit>(e => e.Gender == "Male"))).AnyContext();

        //    // assert
        //    result.ShouldNotBeNull();
        //    result.ShouldNotBeEmpty();
        //}

        //[Fact]
        //public async Task FindAllAsync_WithNotSpecification_Test()
        //{
        //    // arrange/act
        //    var result = await this.sut.FindAllAsync(
        //            new HasEastRegionSpecification()
        //            .And(new Specification<UserVisit>(e => e.Gender == "Male")
        //            .Not())).AnyContext();

        //    // assert
        //    result.ShouldNotBeNull();
        //    result.ShouldNotBeEmpty();
        //}

        //[Fact]
        //public async Task FindAllAsync_WithSpecifications_Test()
        //{
        //    // arrange/act
        //    var result = await this.sut.FindAllAsync(
        //        new[]
        //        {
        //            new HasEastRegionSpecification(),
        //            new Specification<UserVisit>(e => e.Gender == "Male")
        //        }).AnyContext();

        //    // assert
        //    result.ShouldNotBeNull();
        //    result.ShouldNotBeEmpty();
        //}

        [Fact]
        public async Task FindOneAsync_Test()
        {
            // arrange
            var entities = await this.sut.FindAllAsync(
                new FindOptions<UserVisit>(take: 1)).AnyContext();

            // act
            var entity = await this.sut.FindOneAsync(entities.FirstOrDefault()?.Id).AnyContext();

            // assert
            entity.ShouldNotBeNull();
            entity.Email.ShouldNotBeNull();
            entity.Region.ShouldNotBeNull();
            //result.Id.ShouldBe(entities.FirstOrDefault()?.Id);
        }

        [Fact]
        public async Task FindOneAndUpdateAsync_Test()
        {
            // arrange
            var entities = await this.sut.FindAllAsync(
                new FindOptions<UserVisit>(take: 1)).AnyContext();

            // act
            var entity = await this.sut.FindOneAsync(entities.FirstOrDefault()?.Id).AnyContext();
            entity.ShouldNotBeNull();
            entity.State.ShouldNotBeNull();
            entity.Email.ShouldNotBeNull();

            var newEmail = $"{RandomGenerator.GenerateString(5)}@test.com";
            entity.Email = newEmail;
            entity.State.SetUpdated("test", "reason " + new DateTimeEpoch().Epoch);
            await this.sut.UpsertAsync(entity).AnyContext();
            var modifiedEntity = await this.sut.FindOneAsync(entity.Id).AnyContext();

            // assert
            entity.ShouldNotBeNull();
            modifiedEntity.ShouldNotBeNull();
            modifiedEntity.Email.ShouldBe(newEmail);
            modifiedEntity.State.UpdatedReasons.ToString(";").ShouldContain("reason ");
            //result.Id.ShouldBe(entities.FirstOrDefault()?.Id);
        }

        [Fact]
        public async Task FindOneAndDeleteAsync_Test() // SoftDelete because of decorator
        {
            // arrange
            var entities = await this.sut.FindAllAsync(
                new FindOptions<UserVisit>(take: 1)).AnyContext();

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
                result.entity.State.ShouldNotBeNull();
                //result.entity.State.CreatedDescription.ShouldNotBeNull(); // EntityInsertDomainEventHandler
                //result.entity.State.CreatedBy.ShouldNotBeNull(); // EntityInsertDomainEventHandler
            }
        }

        //[Fact]
        //public async Task DeleteAsync_Test()
        //{
        //    // arrange
        //    var entities = await this.sut.FindAllAsync(
        //        new FindOptions<UserAccount>(take: 1)).AnyContext();

        //    // act
        //    await this.sut.DeleteAsync(entities.FirstOrDefault()).AnyContext();
        //    var result = await this.sut.FindOneAsync(entities.FirstOrDefault()?.Id).AnyContext();

        //    // assert
        //    result.ShouldBeNull();
        //}
    }
}
