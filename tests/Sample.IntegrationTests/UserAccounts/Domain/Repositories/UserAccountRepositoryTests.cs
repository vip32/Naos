namespace Naos.Sample.IntegrationTests.Customers.Domain
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Bogus;
    using Naos.Core.Common;
    using Naos.Core.Domain.Repositories;
    using Naos.Core.Domain.Specifications;
    using Naos.Sample.UserAccounts.Domain;
    using Shouldly;
    using Xunit;

    public class UserAccountRepositoryTests : BaseTest
    {
        // https://xunit.github.io/docs/shared-context.html
        private readonly IUserAccountRepository sut;
        private readonly Faker<UserAccount> entityFaker;
        private readonly string tenantId = "naos_sample_test";

        public UserAccountRepositoryTests()
        {
            this.sut = this.container.GetInstance<IUserAccountRepository>();
            this.entityFaker = new Faker<UserAccount>() //https://github.com/bchavez/Bogus
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email())
                .RuleFor(u => u.LastVisitDate, (f, u) => new DateTimeEpoch())
                .RuleFor(u => u.RegisterDate, (f, u) => new DateTimeEpoch())
                .RuleFor(u => u.TenantId, (f, u) => this.tenantId)
                .RuleFor(u => u.VisitCount, (f, u) => 1);
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

        //[Fact]
        //public async Task FindAllAsync_WithOptions_Test()
        //{
        //    // arrange/act
        //    var result = await this.sut.FindAllAsync(
        //        new FindOptions<UserAccount>(take: 3)).ConfigureAwait(false);

        //    // assert
        //    result.ShouldNotBeNull();
        //    result.ShouldNotBeEmpty();
        //    result.Count().ShouldBe(3);
        //}

        //[Fact]
        //public async Task FindAllAsync_WithTenantExtension_Test()
        //{
        //    // arrange/act
        //    var result = await this.sut.FindAllAsync(this.tenantId, default).ConfigureAwait(false);

        //    // assert
        //    result.ShouldNotBeNull();
        //    result.ShouldNotBeEmpty();
        //}

        //[Fact]
        //public async Task FindAllAsync_WithSpecification_Test()
        //{
        //    // arrange/act
        //    var result = await this.sut.FindAllAsync(
        //        new HasEastRegionSpecification()).ConfigureAwait(false);

        //    // assert
        //    result.ShouldNotBeNull();
        //    result.ShouldNotBeEmpty();

        //    // arrange/act
        //    result = await this.sut.FindAllAsync(
        //        new Specification<UserAccount>(e => e.Gender == "Male")).ConfigureAwait(false);

        //    // assert
        //    result.ShouldNotBeNull();
        //    result.ShouldNotBeEmpty(); // fails because of gender enum (=0 instead of Male)
        //}

        //[Fact]
        //public async Task FindAllAsync_WithAndSpecification_Test()
        //{
        //    // arrange/act
        //    var result = await this.sut.FindAllAsync(
        //            new HasEastRegionSpecification()
        //            .And(new Specification<UserAccount>(e => e.Gender == "Male"))).ConfigureAwait(false);

        //    // assert
        //    result.ShouldNotBeNull();
        //    result.ShouldNotBeEmpty();
        //}

        //[Fact]
        //public async Task FindAllAsync_WithOrSpecification_Test()
        //{
        //    // arrange/act
        //    var result = await this.sut.FindAllAsync(
        //            new HasEastRegionSpecification()
        //            .Or(new Specification<UserAccount>(e => e.Gender == "Male"))).ConfigureAwait(false);

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
        //            .Not())).ConfigureAwait(false);

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
        //        }).ConfigureAwait(false);

        //    // assert
        //    result.ShouldNotBeNull();
        //    result.ShouldNotBeEmpty();
        //}

        //[Fact]
        //public async Task FindOneAsync_Test()
        //{
        //    // arrange
        //    var entities = await this.sut.FindAllAsync(
        //        new FindOptions<UserAccount>(take: 1)).ConfigureAwait(false);

        //    // act
        //    var result = await this.sut.FindOneAsync(entities.FirstOrDefault()?.Id).ConfigureAwait(false);

        //    // assert
        //    result.ShouldNotBeNull();
        //    result.Id.ShouldBe(entities.FirstOrDefault()?.Id);
        //}

        //[Fact]
        //public async Task InsertAsync_Test()
        //{
        //    // arrange/act
        //    var result = await this.sut.InsertAsync(this.entityFaker.Generate()).ConfigureAwait(false);

        //    // assert
        //    result.ShouldNotBeNull();
        //    result.Id.ShouldNotBeNull();
        //}

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
            }
        }

        //[Fact]
        //public async Task DeleteAsync_Test()
        //{
        //    // arrange
        //    var entities = await this.sut.FindAllAsync(
        //        new FindOptions<UserAccount>(take: 1)).ConfigureAwait(false);

        //    // act
        //    await this.sut.DeleteAsync(entities.FirstOrDefault()).ConfigureAwait(false);
        //    var result = await this.sut.FindOneAsync(entities.FirstOrDefault()?.Id).ConfigureAwait(false);

        //    // assert
        //    result.ShouldBeNull();
        //}
    }
}
