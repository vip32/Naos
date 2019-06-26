namespace Naos.Core.UnitTests.Domain.Repositories
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FizzWare.NBuilder;
    using MediatR;
    using Naos.Foundation;
    using Naos.Foundation.Domain;
    using NSubstitute;
    using Xunit;
    //using Shouldly; TODO

#pragma warning disable SA1649 // File name must match first type name
    public class InMemoryRepositoryDbTests
#pragma warning restore SA1649 // File name must match first type name
    {
        private readonly string tenantId = "TestTenant";
        private readonly IEnumerable<StubEntity> entities;

        public InMemoryRepositoryDbTests()
        {
            //this.entities = Builder<DbStub>
            //    .CreateListOfSize(20).All()
            //    .With(x => x.FullName, $"John {Core.Common.RandomGenerator.GenerateString(5)}")
            //    .With(x => x.ExtTenantId, this.tenantId)
            //    .With(x => x.Country, "USA").Build()
            //    .Concat(new List<DbStub> { new DbStub { Identifier = "Id99", FullName = "John Doe", YearOfBirth = 1980, Country = "USA" } });

            this.entities = Builder<StubEntity>
                .CreateListOfSize(20).All()
                .With(x => x.FirstName, "John")
                .With(x => x.LastName, Foundation.RandomGenerator.GenerateString(5, false))
                .With(x => x.TenantId, this.tenantId)
                .With(x => x.Country, "USA").Build()
                .Concat(new[] { new StubEntity { Id = "Id99", FirstName = "John", LastName = "Doe", Age = 38, Country = "USA", TenantId = this.tenantId } });
        }

        [Fact]
        public async Task FindOneEntity_Test()
        {
            // arrange
            var sut = new InMemoryRepository<StubEntity, StubDb>(o => o
                .Mediator(Substitute.For<IMediator>())
                .Context(new InMemoryContext<StubEntity>(this.entities))
                .Mapper(new AutoMapperEntityMapper(StubEntityMapperConfiguration.Create())),
                e => e.Identifier);

            // act
            var result = await sut.FindOneAsync("Id99").AnyContext();

            // assert
            Assert.NotNull(result);
            Assert.True(result.Id == "Id99");
        }

        [Fact]
        public async Task FindOneTenantEntity_Test()
        {
            // arrange
            var sut = new RepositorySpecificationDecorator<StubEntity>(
                new Specification<StubEntity>(t => t.TenantId == this.tenantId),
                new InMemoryRepository<StubEntity, StubDb>(o => o
                    .Mediator(Substitute.For<IMediator>())
                    .Context(new InMemoryContext<StubEntity>(this.entities))
                    .Mapper(new AutoMapperEntityMapper(StubEntityMapperConfiguration.Create())),
                    e => e.Identifier));

            // act
            var result = await sut.FindOneAsync("Id99").AnyContext();

            // assert
            Assert.NotNull(result);
            Assert.True(result.Id == "Id99");
        }

        [Fact]
        public async Task FindAllEntities_Test()
        {
            // arrange
            var sut = new InMemoryRepository<StubEntity, StubDb>(o => o
                .Mediator(Substitute.For<IMediator>())
                .Context(new InMemoryContext<StubEntity>(this.entities))
                .Mapper(new AutoMapperEntityMapper(StubEntityMapperConfiguration.Create())),
                e => e.Identifier);

            // act
            var result = await sut.FindAllAsync().AnyContext();

            // assert
            Assert.NotNull(result);
            Assert.True(result.All(e => !e.Id.IsNullOrEmpty() && !e.FirstName.IsNullOrEmpty() && !e.LastName.IsNullOrEmpty()));
            Assert.NotNull(result.FirstOrDefault(e => e.FirstName == "John" && e.LastName == "Doe"));
        }

        [Fact]
        public async Task FindAllTenantEntities_Test() // TODO: move to own test class + mocks
        {
            // arrange
            var sut = new RepositorySpecificationDecorator<StubEntity>(
                new Specification<StubEntity>(t => t.TenantId == this.tenantId),
                new InMemoryRepository<StubEntity, StubDb>(o => o
                    .Mediator(Substitute.For<IMediator>())
                    .Context(new InMemoryContext<StubEntity>(this.entities))
                    .Mapper(new AutoMapperEntityMapper(StubEntityMapperConfiguration.Create())),
                    e => e.Identifier));

            // act
            var result = await sut.FindAllAsync().AnyContext();

            // assert
            Assert.False(result.IsNullOrEmpty());
            Assert.Equal(21, result.Count());
            Assert.NotNull(result.FirstOrDefault(e => e.Id == "Id99"));
        }

        [Fact]
        public async Task FindAllTenantEntities2_Test() // TODO: move to own test class + mocks
        {
            // arrange
            var sut = new RepositorySpecificationDecorator<StubEntity>(
                new Specification<StubEntity>(t => t.TenantId == this.tenantId),
                new InMemoryRepository<StubEntity, StubDb>(o => o
                    .Mediator(Substitute.For<IMediator>())
                    .Context(new InMemoryContext<StubEntity>(this.entities))
                    .Mapper(new AutoMapperEntityMapper(StubEntityMapperConfiguration.Create())),
                    e => e.Identifier));

            // act
            var result = await sut.FindAllAsync().AnyContext();

            // assert
            Assert.False(result.IsNullOrEmpty());
            Assert.Equal(21, result.Count());
        }

        //[Fact]
        //public void TenantSpecificationExpressionMap_Test() // TODO: move to own test class + mocks
        //{
        //    // arrange
        //    var spec = new Specification<StubEntity>(t => t.TenantId == this.tenantId);
        //    var expression = spec.ToExpression();

        //    // act
        //    var dtoExpression = StubEntityMapperConfiguration.Create()
        //        .MapExpression<Expression<Func<DbStub, bool>>>(expression);
        //    var result = this.entities.Where(dtoExpression.Compile());

        //    //var mapped = StubEntityMapperConfiguration.Create().Map<Expression<Func<StubEntity, bool>>, Expression<Func<StubDto, bool>>>(expression);
        //    //var result = this.entities.AsQueryable().Where(mapped);

        //    // assert
        //    Assert.NotNull(dtoExpression);
        //    Assert.NotNull(result);
        //    Assert.NotNull(result.FirstOrDefault()?.Identifier);
        //    Assert.NotNull(result.FirstOrDefault()?.ExtTenantId);
        //    Assert.Equal(this.tenantId, result.FirstOrDefault()?.ExtTenantId);
        //}

        [Fact]
        public async Task FindMappedEntitiesWithSpecification_Test()
        {
            // arrange
            var sut = new InMemoryRepository<StubEntity, StubDb>(o => o
                .Mediator(Substitute.For<IMediator>())
                .Context(new InMemoryContext<StubEntity>(this.entities))
                .Mapper(new AutoMapperEntityMapper(StubEntityMapperConfiguration.Create())),
                e => e.Identifier);

            // act
            var result = await sut.FindAllAsync(
                new StubHasNameSpecification("John", "Doe"),
                new FindOptions<StubEntity>(orderExpression: e => e.Country)).AnyContext(); // domain layer
            //var result = await sut.FindAllAsync(
            //    new StubHasIdSpecification("Id99")).AnyContext(); // domain layer

            // assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.NotNull(result.FirstOrDefault()?.Id);
            Assert.NotNull(result.FirstOrDefault(e => !e.FirstName.IsNullOrEmpty() && !e.LastName.IsNullOrEmpty()));
        }

        //[Fact]
        //public async Task FindMappedEntitiesWithIdSpecification_Test() // fails due to HasIdSpecification (Unable to cast object of type 'StubDto' to type 'Naos.Core.Domain.IEntity'.
        //{
        //    // arrange
        //    var mediator = Substitute.For<IMediator>();
        //    var sut = new InMemoryRepository<StubEntity, StubDto>(
        //        mediator,
        //        this.entities,
        //        new RepositoryOptions(
        //            new AutoMapperEntityMapper(StubEntityMapperConfiguration.Create())),
        //        new List<ISpecificationMapper<StubEntity, StubDto>> { /*new StubHasNameSpecificationMapper(),*/ new AutoMapperSpecificationMapper<StubEntity, StubDto>(StubEntityMapperConfiguration.Create()) }); // infrastructure layer

        //    // act
        //    var result = await sut.FindAllAsync(
        //        new HasIdSpecification<StubEntity>("Id99"),
        //        new FindOptions<StubEntity> { OrderBy = e => e.Country }).AnyContext(); // domain layer
        //        //new StubHasIdSpecification("Id99")).AnyContext(); // domain layer

        //    // assert
        //    Assert.NotNull(result);
        //    Assert.True(result.Count() == 1);
        //    Assert.NotNull(result.FirstOrDefault()?.Id);
        //    Assert.NotNull(result.FirstOrDefault(e => !e.FirstName.IsNullOrEmpty() && !e.LastName.IsNullOrEmpty()));
        //}

        [Fact]
        public async Task FindMappedEntityOne_Test()
        {
            // arrange
            var sut = new InMemoryRepository<StubEntity, StubDb>(o => o
                .Mediator(Substitute.For<IMediator>())
                .Context(new InMemoryContext<StubEntity>(this.entities))
                .Mapper(new AutoMapperEntityMapper(StubEntityMapperConfiguration.Create())),
                e => e.Identifier);

            // act
            var result = await sut.FindOneAsync("Id99").AnyContext();

            // assert
            Assert.NotNull(result);
            Assert.True(!result.Id.IsNullOrEmpty() && !result.FirstName.IsNullOrEmpty() && !result.LastName.IsNullOrEmpty());
            Assert.True(result.Id == "Id99" && result.FirstName == "John" && result.LastName == "Doe");
        }
    }
}