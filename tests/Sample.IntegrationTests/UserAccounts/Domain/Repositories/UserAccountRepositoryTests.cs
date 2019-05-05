namespace Naos.Sample.IntegrationTests.UserAccounts.Domain
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Bogus;
    using Microsoft.Extensions.DependencyInjection;
    using Naos.Core.Common;
    using Naos.Core.Domain.Repositories;
    using Naos.Core.Domain.Specifications;
    using Naos.Sample.UserAccounts.Domain;
    using Shouldly;
    using Xunit;

    public class UserAccountRepositoryTests : BaseTest
    {
        // https://xunit.github.io/docs/shared-context.html
        private readonly IRepository<UserAccount> sut;
        private readonly Faker<UserAccount> entityFaker;
        private readonly string tenantId = "naos_sample_test";

        public UserAccountRepositoryTests()
        {
            //this.sut = this.ServiceProvider.GetService<IUserAccountRepository>();
            this.sut = this.ServiceProvider.GetRequiredService<IRepository<UserAccount>>();
            var domains = new[] { "East", "West" };
            this.entityFaker = new Faker<UserAccount>() //https://github.com/bchavez/Bogus
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email())
                .RuleFor(u => u.LastVisitDate, (f, u) => DateTime.UtcNow)
                .RuleFor(u => u.RegisterDate, (f, u) => DateTime.UtcNow.AddDays(-14))
                .RuleFor(u => u.TenantId, (f, u) => this.tenantId)
                .RuleFor(u => u.AdAccount, (f, u) => AdAccount.For(f.PickRandom(new[] { "East", "West" }) + $"\\{f.System.Random.AlphaNumeric(5)}"))
                .RuleFor(u => u.VisitCount, (f, u) => 1);
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
                new FindOptions<UserAccount>(order: new OrderOption<UserAccount>(e => e.AdAccount.Domain))).AnyContext();

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
            result.First().AdAccount.Domain.ShouldBe("East");
            result.Last().AdAccount.Domain.ShouldBe("West");
        }

        //[Fact]
        //public async Task FindAllAsync_WithOptions_Test()
        //{
        //    // arrange/act
        //    var result = await this.sut.FindAllAsync(
        //        new FindOptions<UserAccount>(take: 3)).AnyContext();

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
                new HasDomainSpecification("East")).AnyContext();

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();

            // arrange/act
            result = await this.sut.FindAllAsync(
                new Specification<UserAccount>(e => e.VisitCount > 0)).AnyContext();

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty(); // fails because of gender enum (=0 instead of Male)
        }

        [Fact]
        public async Task FindAllAsync_WithAndSpecification_Test()
        {
            // arrange/act
            var result = await this.sut.FindAllAsync(
                new HasDomainSpecification("East")
                .And(new Specification<UserAccount>(e => e.VisitCount > 0))).AnyContext();

            // assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();

            result = await this.sut.FindAllAsync(new[]
            {
                //new HasDomainSpecification("East"),
                new HasEastDomainSpecification(),
                new Specification<UserAccount>(e => e.VisitCount > 0)
            }).AnyContext();

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
        //            .Or(new Specification<UserAccount>(e => e.Gender == "Male"))).AnyContext();

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
        //            .And(new Specification<UserAccount>(e => e.Gender == "Male")
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
        //            new Specification<UserAccount>(e => e.Gender == "Male")
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
                new FindOptions<UserAccount>(take: 1)).AnyContext();

            // act
            var entity = await this.sut.FindOneAsync(entities.FirstOrDefault()?.Id).AnyContext();

            // assert
            entity.ShouldNotBeNull();
            entity.Email.ShouldNotBeNull();
            entity.AdAccount.ShouldNotBeNull();
            entity.AdAccount.Domain.ShouldNotBeNull();
            //result.Id.ShouldBe(entities.FirstOrDefault()?.Id);
        }

        [Fact]
        public async Task FindOneAndUpdateAsync_Test()
        {
            // arrange
            var entities = await this.sut.FindAllAsync(
                new FindOptions<UserAccount>(take: 1)).AnyContext();

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
            for(var i = 1; i < 10; i++)
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
