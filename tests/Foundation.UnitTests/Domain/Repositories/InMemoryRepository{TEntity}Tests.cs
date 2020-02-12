namespace Naos.Foundation.UnitTests.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using EnsureThat;
    using FizzWare.NBuilder;
    using MediatR;
    using Naos.Foundation;
    using Naos.Foundation.Domain;
    using NSubstitute;
    using Xunit;
    //using Shouldly; TODO

#pragma warning disable SA1649 // File name must match first type name
#pragma warning disable CA2000 // Dispose objects before losing scope (=InMemoryRepository)
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
            var sut = new InMemoryRepository<StubEntityString>(o => o
                .Mediator(Substitute.For<IMediator>())
                .Context(new InMemoryContext<StubEntityString>(this.entities)));

            // act
            var result = await sut.FindAllAsync().AnyContext();

            // assert
            Assert.False(result.IsNullOrEmpty());
            Assert.Equal(this.entities.First().FirstName, result.FirstOrDefault()?.FirstName);
        }

        [Fact]
        public async Task FindAllTenantEntities_Test() // TODO: move to own test class + mocks
        {
            // arrange
            var sut = new RepositorySpecificationDecorator<StubEntityString>(
                new Specification<StubEntityString>(t => t.TenantId == this.tenantId),
                new InMemoryRepository<StubEntityString>(o => o
                    .Mediator(Substitute.For<IMediator>())
                    .Context(new InMemoryContext<StubEntityString>(this.entities))));

            // act
            var result = await sut.FindAllAsync().AnyContext();

            // assert
            Assert.False(result.IsNullOrEmpty());
            Assert.Equal(20, result.Count());
        }

        [Fact]
        public async Task FindAllEntitiesWithSingleSpecification_Test()
        {
            // arrange
            var sut = new InMemoryRepository<StubEntityString>(o => o
                .Mediator(Substitute.For<IMediator>())
                .Context(new InMemoryContext<StubEntityString>(this.entities)));

            // act/assert
            var result = await sut.FindAllAsync(new StubHasNameSpecification(this.entities.First().FirstName)).AnyContext();

            Assert.False(result.IsNullOrEmpty());
            Assert.Equal(this.entities.First().FirstName, result.FirstOrDefault()?.FirstName);

            result = await sut.FindAllAsync(new HasTenantSpecification<StubEntityString>(this.tenantId)).AnyContext();

            Assert.False(result.IsNullOrEmpty());
            Assert.Equal(20, result.Count());

            result = await sut.FindAllAsync(
                new HasTenantSpecification<StubEntityString>(this.tenantId),
                new FindOptions<StubEntityString>(take: 5, orderExpression: e => e.Country)).AnyContext();

            Assert.False(result.IsNullOrEmpty());
            Assert.Equal(5, result.Count());

            result = await sut.FindAllAsync(this.tenantId, default).AnyContext(); // tenant extension method

            Assert.False(result.IsNullOrEmpty());
            Assert.Equal(20, result.Count());
        }

        [Fact]
        public async Task FindAllEntitiesWithMultipleSpecifications_Test()
        {
            // arrange
            var sut = new InMemoryRepository<StubEntityString>(o => o
                .Mediator(Substitute.For<IMediator>())
                .Context(new InMemoryContext<StubEntityString>(this.entities)));

            // act/assert
            var result = await sut.FindAllAsync(
                new List<ISpecification<StubEntityString>>
                {
                    new StubHasNameSpecification(this.entities.First().FirstName), // And
                    new HasTenantSpecification<StubEntityString>(this.tenantId)
                }).AnyContext();

            Assert.False(result.IsNullOrEmpty());
            Assert.Equal("FirstName1", result.FirstOrDefault()?.FirstName);

            result = await sut.FindAllAsync(
                new List<ISpecification<StubEntityString>>
                {
                    new StubHasNameSpecification(this.entities.First().FirstName), // And
                    new StubHasNameSpecification("Unknown")
                }).AnyContext();

            Assert.True(result.IsNullOrEmpty());
        }

        [Fact]
        public async Task FindAllWithAndSpecification_Test()
        {
            // arrange
            var sut = new InMemoryRepository<StubEntityString>(o => o
                .Mediator(Substitute.For<IMediator>())
                .Context(new InMemoryContext<StubEntityString>(this.entities)));

            // act
            var findResults = await sut.FindAllAsync(
                new StubHasNameSpecification(this.entities.First().FirstName)
                .And(new HasTenantSpecification<StubEntityString>(this.tenantId))).AnyContext();

            // assert
            Assert.False(findResults.IsNullOrEmpty());
            Assert.Equal(this.entities.First().FirstName, findResults.FirstOrDefault()?.FirstName);
        }

        [Fact]
        public async Task FindAllWithOrSpecification_Test()
        {
            // arrange
            var sut = new InMemoryRepository<StubEntityString>(o => o
                .Mediator(Substitute.For<IMediator>())
                .Context(new InMemoryContext<StubEntityString>(this.entities)));

            // act
            var findResults = await sut.FindAllAsync(
                new StubHasNameSpecification(this.entities.First().FirstName)
                .Or(new StubHasNameSpecification(this.entities.Last().FirstName))).AnyContext();

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
            var sut = new InMemoryRepository<StubEntityString>(o => o
                .Mediator(Substitute.For<IMediator>())
                .Context(new InMemoryContext<StubEntityString>(this.entities)));

            // act
            var findResults = await sut.FindAllAsync(
                new HasTenantSpecification<StubEntityString>(this.tenantId)
                .And(new StubHasNameSpecification(this.entities.First().FirstName)
                    .Not())).AnyContext();

            // assert
            Assert.False(findResults.IsNullOrEmpty());
            Assert.DoesNotContain(findResults, f => f.FirstName == this.entities.First().FirstName);
        }

        [Fact]
        public async Task FindOneEntityByStringId_Test()
        {
            // arrange
            var sut = new InMemoryRepository<StubEntityString>(o => o
                .Mediator(Substitute.For<IMediator>())
                .Context(new InMemoryContext<StubEntityString>(this.entities)));

            // act/assert
            var id = this.entities.First().Id;
            var result = await sut.FindOneAsync(id).AnyContext();

            Assert.NotNull(result);
            Assert.Equal(this.entities.First().FirstName, result.FirstName);

            result = await sut.FindOneAsync(id, this.tenantId).AnyContext();

            Assert.NotNull(result);
            Assert.Equal(this.entities.First().FirstName, result.FirstName);
        }

        [Fact]
        public async Task FindOneEntityByGuidId_Test()
        {
            // arrange
            var sut = new InMemoryRepository<StubEntityGuid>(o => o
                .Mediator(Substitute.For<IMediator>())
                .Context(new InMemoryContext<StubEntityGuid>(this.entitiesGuid)));

            // act/assert
            var result = await sut.FindOneAsync(this.entitiesGuid.First().Id).AnyContext();

            Assert.NotNull(result);
            Assert.Equal(this.entities.First().FirstName, result.FirstName);

            result = await sut.FindOneAsync(this.entitiesGuid.First().Id, this.tenantId).AnyContext();

            Assert.NotNull(result);
            Assert.Equal(this.entities.First().FirstName, result.FirstName);
        }

        [Fact]
        public async Task InsertNewEntityWithId_EntityIsAdded()
        {
            // arrange
            var mediator = Substitute.For<IMediator>();
            var sut = new InMemoryRepository<StubEntityString>(o => o
                .Mediator(mediator)
                .Context(new InMemoryContext<StubEntityString>(this.entities)));

            // act
            var result = await sut.UpsertAsync(new StubEntityString
            {
                FirstName = "FirstName99",
                Id = "Id99",
                TenantId = this.tenantId
            }).AnyContext();

            var findResult = await sut.FindOneAsync("Id99").AnyContext();

            // assert
            Assert.Equal(ActionResult.Inserted, result.action);
            Assert.False(result.entity.Id.IsNullOrEmpty());
            Assert.False(result.entity.Id.IsDefault());
            Assert.Equal("Id99", result.entity.Id);
            Assert.NotNull(findResult);
            Assert.Equal("FirstName99", findResult.FirstName);
            await mediator.Received().Publish(Arg.Any<IDomainEvent>()).AnyContext();
        }

        [Fact]
        public async Task InsertNewEntityWithoutId_EntityIsAddedWithGeneratedId()
        {
            // arrange
            var mediator = Substitute.For<IMediator>();
            var sut = new InMemoryRepository<StubEntityString>(o => o
                .Mediator(mediator)
                .Context(new InMemoryContext<StubEntityString>(this.entities)));

            // act
            var result = await sut.UpsertAsync(new StubEntityString
            {
                FirstName = "FirstName88",
                TenantId = this.tenantId
            }).AnyContext();

            var findResult = await sut.FindOneAsync(result.entity.Id).AnyContext();

            // assert
            Assert.Equal(ActionResult.Inserted, result.action);
            Assert.NotNull(result.entity);
            Assert.False(result.entity.Id.IsNullOrEmpty());
            Assert.False(result.entity.Id.IsDefault());
            Assert.NotNull(findResult);
            Assert.Equal(findResult.Id, result.entity.Id);
            Assert.Equal("FirstName88", findResult.FirstName);
            await mediator.Received().Publish(Arg.Any<IDomainEvent>()).AnyContext();
        }

        [Fact]
        public async Task UpdateExistingEntityWithId_EntityIsUpdated()
        {
            // arrange
            var mediator = Substitute.For<IMediator>();
            var sut = new InMemoryRepository<StubEntityString>(o => o
                .Mediator(mediator)
                .Context(new InMemoryContext<StubEntityString>(this.entities)));

            // act
            var result = await sut.UpsertAsync(new StubEntityString
            {
                Id = "Id1",
                FirstName = "FirstName77",
                LastName = "LastName77",
                TenantId = this.tenantId
            }).AnyContext();

            var findResult = await sut.FindOneAsync("Id1").AnyContext();

            // assert
            Assert.Equal(ActionResult.Updated, result.action);
            Assert.NotNull(result.entity);
            Assert.False(result.entity.Id.IsNullOrEmpty());
            Assert.False(result.entity.Id.IsDefault());
            Assert.Equal("Id1", result.entity.Id);
            Assert.NotNull(findResult);
            Assert.Equal(findResult.Id, result.entity.Id);
            Assert.Equal("FirstName77", findResult.FirstName);
            await mediator.Received().Publish(Arg.Any<IDomainEvent>()).AnyContext();
        }

        [Fact]
        public async Task DeleteEntityById_Test()
        {
            // arrange
            var mediator = Substitute.For<IMediator>();
            var sut = new InMemoryRepository<StubEntityString>(o => o
                .Mediator(mediator)
                .Context(new InMemoryContext<StubEntityString>(this.entities)));

            // act
            var id = this.entities.First().Id;
            await sut.DeleteAsync(id).AnyContext();
            var entity = await sut.FindOneAsync(id).AnyContext();

            // assert
            Assert.Null(entity);
            await mediator.Received().Publish(Arg.Any<IDomainEvent>()).AnyContext();
        }

        [Fact]
        public async Task DeleteEntity_Test()
        {
            // arrange
            var mediator = Substitute.For<IMediator>();
            var sut = new InMemoryRepository<StubEntityString>(o => o
                .Mediator(mediator)
                .Context(new InMemoryContext<StubEntityString>(this.entities)));

            // act
            var entity = this.entities.FirstOrDefault(e => e.FirstName == this.entities.First().FirstName);
            await sut.DeleteAsync(entity).AnyContext();
            entity = await sut.FindOneAsync(entity.Id).AnyContext();

            // assert
            Assert.Null(entity);
            await mediator.Received().Publish(Arg.Any<IDomainEvent>()).AnyContext();
        }

        public class StubEntityString : TenantAggregateRoot<string>
        {
            public string Country { get; set; }

            public string FirstName { get; set; }

            public string LastName { get; set; }

            public int Age { get; set; }
        }

        public class StubEntityGuid : TenantAggregateRoot<Guid>
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