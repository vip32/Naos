namespace Naos.Core.UnitTests.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using EnsureThat;
    using FizzWare.NBuilder;
    using MediatR;
    using Moq;
    using Naos.Core.Common;
    using Naos.Core.Domain;
    using Xunit;

    public class InMemoryRepositoryTests
    {
        private readonly string tenantId = "TestTenant";
        private readonly IEnumerable<StubEntityString> entities;
        private readonly IEnumerable<StubEntityGuid> guidEntities;

        public InMemoryRepositoryTests()
        {
            this.entities = Builder<StubEntityString>
                .CreateListOfSize(20).All()
                .With(x => x.TenantId, this.tenantId).Build();

            this.guidEntities = Builder<StubEntityGuid>
                .CreateListOfSize(20).All()
                .With(x => x.TenantId, this.tenantId).Build();
        }

        [Fact]
        public async Task FindAllEntities_Test()
        {
            // arrange
            var mediatorMock = new Mock<IMediator>();
            var sut = InMemoryRepositoryFactory.CreateForStringId(mediatorMock.Object, this.entities);

            // act
            var result = await sut.FindAllAsync().ConfigureAwait(false);

            // assert
            var stubEntities = result as StubEntityString[] ?? result.ToArray();
            Assert.False(stubEntities.IsNullOrEmpty());
            Assert.Equal("FirstName1", stubEntities.FirstOrDefault()?.FirstName);
        }

        [Fact]
        public async Task FindAllEntitiesWithSingleSpecification_Test()
        {
            // arrange
            var mediatorMock = new Mock<IMediator>();
            var sut = InMemoryRepositoryFactory.CreateForStringId(mediatorMock.Object, this.entities);

            // act/assert
            var result = await sut.FindAllAsync(new StubEntityHasNameSpecification("FirstName1")).ConfigureAwait(false);

            var stubEntities = result as StubEntityString[] ?? result.ToArray();
            Assert.False(stubEntities.IsNullOrEmpty());
            Assert.Equal("FirstName1", stubEntities.FirstOrDefault()?.FirstName);

            result = await sut.FindAllAsync(new StubEntityHasTenantSpecification(this.tenantId)).ConfigureAwait(false);

            stubEntities = result as StubEntityString[] ?? result.ToArray();
            Assert.False(stubEntities.IsNullOrEmpty());
            Assert.True(stubEntities.Length == 20);

            result = await sut.FindAllAsync(new HasTenantSpecification<StubEntityString, string>(this.tenantId)).ConfigureAwait(false);

            stubEntities = result as StubEntityString[] ?? result.ToArray();
            Assert.False(stubEntities.IsNullOrEmpty());
            Assert.True(stubEntities.Length == 20);

            result = await sut.FindAllAsync(this.tenantId).ConfigureAwait(false); // tenant extension method

            stubEntities = result as StubEntityString[] ?? result.ToArray();
            Assert.False(stubEntities.IsNullOrEmpty());
            Assert.True(stubEntities.Length == 20);
        }

        [Fact]
        public async Task FindAllEntitiesWithMultipleSpecifications_Test()
        {
            // arrange
            var mediatorMock = new Mock<IMediator>();
            var sut = InMemoryRepositoryFactory.CreateForStringId(mediatorMock.Object, this.entities);

            // act/assert
            var result = await sut.FindAllAsync(
                new List<ISpecification<StubEntityString>>
                {
                    new StubEntityHasNameSpecification("FirstName1"), // And
                    new StubEntityHasTenantSpecification(this.tenantId)
                }).ConfigureAwait(false);

            var stubEntities = result as StubEntityString[] ?? result.ToArray();
            Assert.False(stubEntities.IsNullOrEmpty());
            Assert.Equal("FirstName1", stubEntities.FirstOrDefault()?.FirstName);

            result = await sut.FindAllAsync(
                new List<ISpecification<StubEntityString>>
                {
                    new StubEntityHasNameSpecification("FirstName1"), // And
                    new StubEntityHasNameSpecification("Unknown")
                }).ConfigureAwait(false);

            Assert.True(result.IsNullOrEmpty());
        }

        [Fact]
        public async Task FindEntityByStringId_Test()
        {
            // arrange
            var mediatorMock = new Mock<IMediator>();
            var sut = InMemoryRepositoryFactory.CreateForStringId(mediatorMock.Object, this.entities);

            // act/assert
            var result = await sut.FindByIdAsync("Id1").ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.Equal("FirstName1", result.FirstName);

            result = await sut.FindByIdAsync("Id1", this.tenantId).ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.Equal("FirstName1", result.FirstName);
        }

        [Fact]
        public async Task FindEntityByGuidId_Test()
        {
            // arrange
            var mediatorMock = new Mock<IMediator>();
            var sut = InMemoryRepositoryFactory.CreateForGuidId(mediatorMock.Object, this.guidEntities);

            // act/assert
            var result = await sut.FindByIdAsync(this.guidEntities.First().Id).ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.Equal("FirstName1", result.FirstName);

            result = await sut.FindByIdAsync(this.guidEntities.First().Id, this.tenantId).ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.Equal("FirstName1", result.FirstName);
        }

        [Fact]
        public async Task AddOrUpdateEntity_Test()
        {
            // arrange
            var mediatorMock = new Mock<IMediator>();
            var sut = InMemoryRepositoryFactory.CreateForStringId(mediatorMock.Object, this.entities);

            // act
            var entity = await sut.AddOrUpdateAsync(new StubEntityString
            {
                FirstName = "FirstName99",
                Id = "Id99",
                TenantId = this.tenantId
            }).ConfigureAwait(false);

            // assert
            Assert.NotNull(entity);
            Assert.False(entity.Id.IsNullOrEmpty());
            Assert.Equal("FirstName99", entity.FirstName);
        }

        [Fact]
        public async Task DeleteEntityById_Test()
        {
            // arrange
            var mediatorMock = new Mock<IMediator>();
            var sut = InMemoryRepositoryFactory.CreateForStringId(mediatorMock.Object, this.entities);

            // act
            await sut.DeleteAsync("Id1").ConfigureAwait(false);
            var entity = await sut.FindByIdAsync("Id1").ConfigureAwait(false);

            // assert
            Assert.Null(entity);
        }

        [Fact]
        public async Task DeleteEntity_Test()
        {
            // arrange
            var mediatorMock = new Mock<IMediator>();
            var sut = InMemoryRepositoryFactory.CreateForStringId(mediatorMock.Object, this.entities);

            // act
            var entity = this.entities.FirstOrDefault(e => e.FirstName == "FirstName1");
            await sut.DeleteAsync(entity).ConfigureAwait(false);
            entity = await sut.FindByIdAsync("Id1").ConfigureAwait(false);

            // assert
            Assert.Null(entity);
        }
    }

#pragma warning disable SA1402 // File may only contain a single class
    public class StubEntityString : TenantEntity<string>, IAggregateRoot
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int Age { get; set; }
    }

    public class StubEntityGuid : TenantEntity<Guid>, IAggregateRoot
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int Age { get; set; }
    }

    public class StubEntityHasNameSpecification : Specification<StubEntityString> // TODO: this should be mocked
    {
        private readonly string firstName;

        public StubEntityHasNameSpecification(string firstName)
        {
            EnsureArg.IsNotNull(firstName);

            this.firstName = firstName;
        }

        public override Expression<Func<StubEntityString, bool>> Expression()
        {
            return p => p.FirstName == this.firstName;
        }
    }

    public class StubEntityHasTenantSpecification : HasTenantSpecification<StubEntityString, string> // TODO: this should be mocked
    {
        public StubEntityHasTenantSpecification(string tenantId)
            : base(tenantId)
        {
        }
    }

    public class StubEntityGuidHasNameSpecification : Specification<StubEntityGuid> // TODO: this should be mocked
    {
        private readonly string firstName;

        public StubEntityGuidHasNameSpecification(string firstName)
        {
            EnsureArg.IsNotNull(firstName);

            this.firstName = firstName;
        }

        public override Expression<Func<StubEntityGuid, bool>> Expression()
        {
            return p => p.FirstName == this.firstName;
        }
    }

    public class StubEntityGuidHasTenantSpecification : HasTenantSpecification<StubEntityGuid, Guid> // TODO: this should be mocked
    {
        public StubEntityGuidHasTenantSpecification(string tenantId)
            : base(tenantId)
        {
        }
    }
#pragma warning restore SA1402 // File may only contain a single class
}