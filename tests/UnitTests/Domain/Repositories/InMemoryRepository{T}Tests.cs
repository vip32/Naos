namespace Naos.Core.UnitTests.Domain.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using EnsureThat;
    using FizzWare.NBuilder;
    using MediatR;
    using Naos.Core.Common;
    using Naos.Core.Domain;
    using Naos.Core.Domain.Repositories;
    using Naos.Core.Domain.Specifications;
    using NSubstitute;
    using Xunit;

#pragma warning disable SA1649 // File name must match first type name
    public class InMemoryRepositoryTests
#pragma warning restore SA1649 // File name must match first type name
    {
        private readonly string tenantId = "TestTenant";
        private readonly IEnumerable<StubEntityString> entities;
        private readonly IEnumerable<StubEntityGuid> entitiesGuid;

        public InMemoryRepositoryTests()
        {
            this.entities = Builder<StubEntityString>
                .CreateListOfSize(20).All()
                .With(x => x.TenantId, this.tenantId)
                .With(x => x.Country, "USA").Build();

            this.entitiesGuid = Builder<StubEntityGuid>
                .CreateListOfSize(20).All()
                .With(x => x.TenantId, this.tenantId)
                .With(x => x.Country, "USA").Build();
        }

        [Fact]
        public async Task FindAllEntities_Test()
        {
            // arrange
            var mediator = Substitute.For<IMediator>();
            var sut = new InMemoryRepository<StubEntityString>(mediator, this.entities);

            // act
            var result = await sut.FindAllAsync().ConfigureAwait(false);

            // assert
            Assert.False(result.IsNullOrEmpty());
            Assert.Equal(this.entities.First().FirstName, result.FirstOrDefault()?.FirstName);
        }

        [Fact]
        public async Task FindAllTenantEntities_Test() // TODO: move to own test class + mocks
        {
            // arrange
            var mediator = Substitute.For<IMediator>();
            var sut = new RepositoryTenantDecorator<StubEntityString>(
                new InMemoryRepository<StubEntityString>(mediator, this.entities), this.tenantId); // = decoratee

            // act
            var result = await sut.FindAllAsync().ConfigureAwait(false);

            // assert
            Assert.False(result.IsNullOrEmpty());
            Assert.Equal(20, result.Count());
        }

        [Fact]
        public async Task FindAllEntitiesWithSingleSpecification_Test()
        {
            // arrange
            var mediator = Substitute.For<IMediator>();
            var sut = new InMemoryRepository<StubEntityString>(mediator, this.entities);

            // act/assert
            var result = await sut.FindAllAsync(new StubHasNameSpecification(this.entities.First().FirstName)).ConfigureAwait(false);

            Assert.False(result.IsNullOrEmpty());
            Assert.Equal(this.entities.First().FirstName, result.FirstOrDefault()?.FirstName);

            result = await sut.FindAllAsync(new HasTenantSpecification<StubEntityString>(this.tenantId)).ConfigureAwait(false);

            Assert.False(result.IsNullOrEmpty());
            Assert.Equal(20, result.Count());

            result = await sut.FindAllAsync(
                new HasTenantSpecification<StubEntityString>(this.tenantId),
                new FindOptions<StubEntityString>(take: 5) { OrderBy = e => e.Country}).ConfigureAwait(false);

            Assert.False(result.IsNullOrEmpty());
            Assert.Equal(5, result.Count());

            result = await sut.FindAllAsync(this.tenantId, default).ConfigureAwait(false); // tenant extension method

            Assert.False(result.IsNullOrEmpty());
            Assert.Equal(20, result.Count());
        }

        [Fact]
        public async Task FindAllEntitiesWithMultipleSpecifications_Test()
        {
            // arrange
            var mediator = Substitute.For<IMediator>();
            var sut = new InMemoryRepository<StubEntityString>(mediator, this.entities);

            // act/assert
            var result = await sut.FindAllAsync(
                new List<ISpecification<StubEntityString>>
                {
                    new StubHasNameSpecification(this.entities.First().FirstName), // And
                    new HasTenantSpecification<StubEntityString>(this.tenantId)
                }).ConfigureAwait(false);

            Assert.False(result.IsNullOrEmpty());
            Assert.Equal("FirstName1", result.FirstOrDefault()?.FirstName);

            result = await sut.FindAllAsync(
                new List<ISpecification<StubEntityString>>
                {
                    new StubHasNameSpecification(this.entities.First().FirstName), // And
                    new StubHasNameSpecification("Unknown")
                }).ConfigureAwait(false);

            Assert.True(result.IsNullOrEmpty());
        }

        [Fact]
        public async Task FindAllWithAndSpecification_Test()
        {
            // arrange
            var mediator = Substitute.For<IMediator>();
            var sut = new InMemoryRepository<StubEntityString>(mediator, this.entities);

            // act
            var findResults = await sut.FindAllAsync(
                new StubHasNameSpecification(this.entities.First().FirstName)
                .And(new HasTenantSpecification<StubEntityString>(this.tenantId))).ConfigureAwait(false);

            // assert
            Assert.False(findResults.IsNullOrEmpty());
            Assert.Equal(this.entities.First().FirstName, findResults.FirstOrDefault()?.FirstName);
        }

        [Fact]
        public async Task FindAllWithOrSpecification_Test()
        {
            // arrange
            var mediator = Substitute.For<IMediator>();
            var sut = new InMemoryRepository<StubEntityString>(mediator, this.entities);

            // act
            var findResults = await sut.FindAllAsync(
                new StubHasNameSpecification(this.entities.First().FirstName)
                .Or(new StubHasNameSpecification(this.entities.Last().FirstName))).ConfigureAwait(false);

            // assert
            Assert.False(findResults.IsNullOrEmpty());
            Assert.Equal(2, findResults.Count());
            Assert.Contains(findResults, f => f.FirstName == this.entities.First().FirstName);
            Assert.Contains(findResults, f => f.FirstName == this.entities.Last().FirstName);
        }

        [Fact]
        public async Task FindAllWithNotSpecification_Test()
        {
            // arrange
            var mediator = Substitute.For<IMediator>();
            var sut = new InMemoryRepository<StubEntityString>(mediator, this.entities);

            // act
            var findResults = await sut.FindAllAsync(
                new HasTenantSpecification<StubEntityString>(this.tenantId)
                .And(new StubHasNameSpecification(this.entities.First().FirstName)
                    .Not())).ConfigureAwait(false);

            // assert
            Assert.False(findResults.IsNullOrEmpty());
            Assert.DoesNotContain(findResults, f => f.FirstName == this.entities.First().FirstName);
        }

        [Fact]
        public async Task FindEntityByStringId_Test()
        {
            // arrange
            var mediator = Substitute.For<IMediator>();
            var sut = new InMemoryRepository<StubEntityString>(mediator, this.entities);

            // act/assert
            var id = this.entities.First().Id;
            var result = await sut.FindOneAsync(id).ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.Equal(this.entities.First().FirstName, result.FirstName);

            result = await sut.FindAsync(this.entities.First().Id, this.tenantId).ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.Equal(this.entities.First().FirstName, result.FirstName);
        }

        [Fact]
        public async Task FindEntityByGuidId_Test()
        {
            // arrange
            var mediator = Substitute.For<IMediator>();
            var sut = new InMemoryRepository<StubEntityGuid>(mediator, this.entitiesGuid);

            // act/assert
            var result = await sut.FindOneAsync(this.entitiesGuid.First().Id).ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.Equal(this.entities.First().FirstName, result.FirstName);

            result = await sut.FindAsync(this.entitiesGuid.First().Id, this.tenantId).ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.Equal(this.entities.First().FirstName, result.FirstName);
        }

        [Fact]
        public async Task InsertNewEntityWithId_EntityIsAdded()
        {
            // arrange
            var mediator = Substitute.For<IMediator>();
            var sut = new InMemoryRepository<StubEntityString>(mediator, this.entities);

            // act
            var result = await sut.UpsertAsync(new StubEntityString
            {
                FirstName = "FirstName99",
                Id = "Id99",
                TenantId = this.tenantId
            }).ConfigureAwait(false);

            var findResult = await sut.FindOneAsync("Id99").ConfigureAwait(false);

            // assert
            Assert.Equal(UpsertAction.Inserted, result.action);
            Assert.False(result.entity.Id.IsNullOrEmpty());
            Assert.False(result.entity.Id.IsDefault());
            Assert.Equal("Id99", result.entity.Id);
            Assert.NotNull(findResult);
            Assert.Equal("FirstName99", findResult.FirstName);
            await mediator.Received().Publish(Arg.Any<IDomainEvent>());
        }

        [Fact]
        public async Task InsertNewEntityWithoutId_EntityIsAddedWithGeneratedId()
        {
            // arrange
            var mediator = Substitute.For<IMediator>();
            var sut = new InMemoryRepository<StubEntityString>(mediator, this.entities);

            // act
            var result = await sut.UpsertAsync(new StubEntityString
            {
                FirstName = "FirstName88",
                TenantId = this.tenantId
            }).ConfigureAwait(false);

            var findResult = await sut.FindOneAsync(result.entity.Id).ConfigureAwait(false);

            // assert
            Assert.Equal(UpsertAction.Inserted, result.action);
            Assert.NotNull(result.entity);
            Assert.False(result.entity.Id.IsNullOrEmpty());
            Assert.False(result.entity.Id.IsDefault());
            Assert.NotNull(findResult);
            Assert.Equal(findResult.Id, result.entity.Id);
            Assert.Equal("FirstName88", findResult.FirstName);
            await mediator.Received().Publish(Arg.Any<IDomainEvent>());
        }

        [Fact]
        public async Task UpdateExistingEntityWithId_EntityIsUpdated()
        {
            // arrange
            var mediator = Substitute.For<IMediator>();
            var sut = new InMemoryRepository<StubEntityString>(mediator, this.entities);

            // act
            var result = await sut.UpsertAsync(new StubEntityString
            {
                Id = "Id1",
                FirstName = "FirstName77",
                LastName = "LastName77",
                TenantId = this.tenantId
            }).ConfigureAwait(false);

            var findResult = await sut.FindOneAsync("Id1").ConfigureAwait(false);

            // assert
            Assert.Equal(UpsertAction.Updated, result.action);
            Assert.NotNull(result.entity);
            Assert.False(result.entity.Id.IsNullOrEmpty());
            Assert.False(result.entity.Id.IsDefault());
            Assert.Equal("Id1", result.entity.Id);
            Assert.NotNull(findResult);
            Assert.Equal(findResult.Id, result.entity.Id);
            Assert.Equal("FirstName77", findResult.FirstName);
            await mediator.Received().Publish(Arg.Any<IDomainEvent>());
        }

        [Fact]
        public async Task DeleteEntityById_Test()
        {
            // arrange
            var mediator = Substitute.For<IMediator>();
            var sut = new InMemoryRepository<StubEntityString>(mediator, this.entities);

            // act
            var id = this.entities.First().Id;
            await sut.DeleteAsync(id).ConfigureAwait(false);
            var entity = await sut.FindOneAsync(id).ConfigureAwait(false);

            // assert
            Assert.Null(entity);
            await mediator.Received().Publish(Arg.Any<IDomainEvent>());
        }

        [Fact]
        public async Task DeleteEntity_Test()
        {
            // arrange
            var mediator = Substitute.For<IMediator>();
            var sut = new InMemoryRepository<StubEntityString>(mediator, this.entities);

            // act
            var entity = this.entities.FirstOrDefault(e => e.FirstName == this.entities.First().FirstName);
            await sut.DeleteAsync(entity).ConfigureAwait(false);
            entity = await sut.FindOneAsync(entity.Id).ConfigureAwait(false);

            // assert
            Assert.Null(entity);
            await mediator.Received().Publish(Arg.Any<IDomainEvent>());
        }

        public class StubEntityString : TenantEntity<string>, IAggregateRoot
        {
            public string Country { get; set; }

            public string FirstName { get; set; }

            public string LastName { get; set; }

            public int Age { get; set; }
        }

        public class StubEntityGuid : TenantEntity<Guid>, IAggregateRoot
        {
            public string Country { get; set; }

            public string FirstName { get; set; }

            public string LastName { get; set; }

            public int Age { get; set; }
        }

        public class StubHasNameSpecification : Specification<StubEntityString> // TODO: this should be mocked
        {
            private readonly string firstName;

            public StubHasNameSpecification(string firstName)
            {
                EnsureArg.IsNotNull(firstName);

                this.firstName = firstName;
            }

            public override Expression<Func<StubEntityString, bool>> ToExpression()
            {
                return p => p.FirstName == this.firstName;
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

            public override Expression<Func<StubEntityGuid, bool>> ToExpression()
            {
                return p => p.FirstName == this.firstName;
            }
        }
    }
}