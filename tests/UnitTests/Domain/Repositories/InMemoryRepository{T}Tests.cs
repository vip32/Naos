namespace Naos.Core.UnitTests.Domain.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
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
        private readonly IEnumerable<StubEntityGuid> guidEntities;

        public InMemoryRepositoryTests()
        {
            this.entities = Builder<StubEntityString>
                .CreateListOfSize(20).All()
                .With(x => x.TenantId, this.tenantId)
                .With(x => x.Country, "USA").Build();

            this.guidEntities = Builder<StubEntityGuid>
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
            var stubEntities = result as StubEntityString[] ?? result.ToArray();
            Assert.False(stubEntities.IsNullOrEmpty());
            Assert.Equal(this.entities.First().FirstName, stubEntities.FirstOrDefault()?.FirstName);
        }

        [Fact]
        public async Task FindAllEntitiesWithSingleSpecification_Test()
        {
            // arrange
            var mediator = Substitute.For<IMediator>();
            var sut = new InMemoryRepository<StubEntityString>(mediator, this.entities);

            // act/assert
            var result = await sut.FindAllAsync(new StubHasNameSpecification(this.entities.First().FirstName)).ConfigureAwait(false);

            var stubEntities = result as StubEntityString[] ?? result.ToArray();
            Assert.False(stubEntities.IsNullOrEmpty());
            Assert.Equal(this.entities.First().FirstName, stubEntities.FirstOrDefault()?.FirstName);

            result = await sut.FindAllAsync(new StubHasTenantSpecification(this.tenantId)).ConfigureAwait(false);

            stubEntities = result as StubEntityString[] ?? result.ToArray();
            Assert.False(stubEntities.IsNullOrEmpty());
            Assert.True(stubEntities.Length == 20);

            result = await sut.FindAllAsync(
                new HasTenantSpecification<StubEntityString>(this.tenantId),
                new FindOptions<StubEntityString>(take: 5) { OrderBy = e => e.Country}).ConfigureAwait(false);

            stubEntities = result as StubEntityString[] ?? result.ToArray();
            Assert.False(stubEntities.IsNullOrEmpty());
            Assert.True(stubEntities.Length == 5);

            result = await sut.FindAllAsync(this.tenantId, default).ConfigureAwait(false); // tenant extension method

            stubEntities = result as StubEntityString[] ?? result.ToArray();
            Assert.False(stubEntities.IsNullOrEmpty());
            Assert.True(stubEntities.Length == 20);
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
                    new StubHasTenantSpecification(this.tenantId)
                }).ConfigureAwait(false);

            var stubEntities = result as StubEntityString[] ?? result.ToArray();
            Assert.False(stubEntities.IsNullOrEmpty());
            Assert.Equal("FirstName1", stubEntities.FirstOrDefault()?.FirstName);

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
                .And(new StubHasTenantSpecification(this.tenantId))).ConfigureAwait(false);

            // assert
            var findResultsArray = findResults as StubEntityString[] ?? findResults.ToArray();
            Assert.False(findResultsArray.IsNullOrEmpty());
            Assert.Equal(this.entities.First().FirstName, findResultsArray.FirstOrDefault()?.FirstName);
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
            var findResultsArray = findResults as StubEntityString[] ?? findResults.ToArray();
            Assert.False(findResultsArray.IsNullOrEmpty());
            Assert.True(findResultsArray.Count() == 2);
            Assert.Contains(findResultsArray, f => f.FirstName == this.entities.First().FirstName);
            Assert.Contains(findResultsArray, f => f.FirstName == this.entities.Last().FirstName);
        }

        [Fact]
        public async Task FindAllWithNotSpecification_Test()
        {
            // arrange
            var mediator = Substitute.For<IMediator>();
            var sut = new InMemoryRepository<StubEntityString>(mediator, this.entities);

            // act
            var findResults = await sut.FindAllAsync(
                new StubHasTenantSpecification(this.tenantId)
                .And(new StubHasNameSpecification(this.entities.First().FirstName)
                    .Not())).ConfigureAwait(false);

            // assert
            var findResultsArray = findResults as StubEntityString[] ?? findResults.ToArray();
            Assert.False(findResultsArray.IsNullOrEmpty());
            Assert.DoesNotContain(findResultsArray, f => f.FirstName == this.entities.First().FirstName);
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
            var sut = new InMemoryRepository<StubEntityGuid>(mediator, this.guidEntities);

            // act/assert
            var result = await sut.FindOneAsync(this.guidEntities.First().Id).ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.Equal(this.entities.First().FirstName, result.FirstName);

            result = await sut.FindAsync(this.guidEntities.First().Id, this.tenantId).ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.Equal(this.entities.First().FirstName, result.FirstName);
        }

        [Fact]
        public async Task AddNewEntityWithId_EntityIsAdded()
        {
            // arrange
            var mediator = Substitute.For<IMediator>();
            var sut = new InMemoryRepository<StubEntityString>(mediator, this.entities);

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
            await mediator.Received().Publish(Arg.Any<IDomainEvent>());
        }

        [Fact]
        public async Task AddNewEntityWithoutId_EntityIsAddedWithGeneratedId()
        {
            // arrange
            var mediator = Substitute.For<IMediator>();
            var sut = new InMemoryRepository<StubEntityString>(mediator, this.entities);

            // act
            var entity = await sut.AddOrUpdateAsync(new StubEntityString
            {
                FirstName = "FirstName88",
                TenantId = this.tenantId
            }).ConfigureAwait(false);

            // assert
            Assert.NotNull(entity);
            Assert.False(entity.Id.IsNullOrEmpty());
            Assert.Equal("FirstName88", entity.FirstName);
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

        public class StubHasTenantSpecification : HasTenantSpecification<StubEntityString> // TODO: this should be mocked
        {
            public StubHasTenantSpecification(string tenantId)
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

            public override Expression<Func<StubEntityGuid, bool>> ToExpression()
            {
                return p => p.FirstName == this.firstName;
            }
        }

        public class StubEntityGuidHasTenantSpecification : HasTenantSpecification<StubEntityGuid> // TODO: this should be mocked
        {
            public StubEntityGuidHasTenantSpecification(string tenantId)
                : base(tenantId)
            {
            }
        }
    }
}