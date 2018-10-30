namespace Naos.Sample.App.IntegrationTests.Infrastructure.Azure.CosmosDb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using MediatR;
    using Naos.Core.Common;
    using Naos.Core.Domain;
    using Naos.Core.Domain.Repositories;
    using Naos.Core.Domain.Specifications;
    using Naos.Core.Infrastructure.Azure.CosmosDb;
    using NSubstitute;
    using Xunit;

    public class CosmosDbSqlRepositoryTests : BaseTest
    {
        private readonly StubCosmosDbSqlRepository repository;
        private readonly IMediator mediator = Substitute.For<IMediator>();
        private readonly string tenantId = "naos_test";

        public CosmosDbSqlRepositoryTests()
        {
            this.repository = new StubCosmosDbSqlRepository(
                this.mediator,
                new CosmosDbSqlProvider<StubEntity>(
                        client: CosmosDbClient.Create(AppConfiguration.CosmosDb.ServiceEndpointUri, AppConfiguration.CosmosDb.AuthKeyOrResourceToken),
                        databaseId: AppConfiguration.CosmosDb.DatabaseId,
                        collectionNameFactory: () => AppConfiguration.CosmosDb.CollectionName,
                        collectionPartitionKey: AppConfiguration.CosmosDb.CollectionPartitionKey,
                        collectionOfferThroughput: AppConfiguration.CosmosDb.CollectionOfferThroughput,
                        isMasterCollection: AppConfiguration.CosmosDb.IsMasterCollection));
        }

        [Fact]
        public async Task FindAll_Test()
        {
            // arrange
            var sut = this.repository;

            // act
            var findResults = await sut.FindAllAsync().ConfigureAwait(false);

            // assert
            var findResultsArray = findResults as StubEntity[] ?? findResults.ToArray();
            Assert.False(findResultsArray.IsNullOrEmpty());
            Assert.Equal("Name1", findResultsArray.FirstOrDefault()?.Name);

            findResults = await sut.FindAllAsync(this.tenantId, default).ConfigureAwait(false);

            findResultsArray = findResults as StubEntity[] ?? findResults.ToArray();
            Assert.False(findResultsArray.IsNullOrEmpty());
            Assert.Equal("Name1", findResultsArray.FirstOrDefault()?.Name);
        }

        [Fact]
        public async Task FindAllWithSpecification_Test()
        {
            // arrange
            var sut = this.repository;

            // act
            var findResults = await sut.FindAllAsync(new StubHasNameSpecification("Name1")).ConfigureAwait(false);

            // assert
            var findResultsArray = findResults as StubEntity[] ?? findResults.ToArray();
            Assert.False(findResultsArray.IsNullOrEmpty());
            Assert.Equal("Name1", findResultsArray.FirstOrDefault()?.Name);

            findResults = await sut.FindAllAsync(new StubHasTenantSpecification(this.tenantId)).ConfigureAwait(false);

            findResultsArray = findResults as StubEntity[] ?? findResults.ToArray();
            Assert.False(findResultsArray.IsNullOrEmpty());
            Assert.True(findResultsArray.Length == 20);

            findResults = await sut.FindAllAsync(
                new HasTenantSpecification<StubEntity>(this.tenantId),
                new FindOptions<StubEntity>(take: 5)).ConfigureAwait(false);

            //findResultsArray = findResults as StubEntity[] ?? findResults.ToArray();
            //Assert.False(findResultsArray.IsNullOrEmpty());
            //Assert.True(findResultsArray.Length == 5); // FAILS at the moment due to not supported skip/take in cosmosdb // TODO: implement cosmosdb skip/take once available https://feedback.azure.com/forums/263030-azure-cosmos-db/suggestions/6350987--documentdb-allow-paging-skip-take

            findResults = await sut.FindAllAsync(this.tenantId, default).ConfigureAwait(false); // tenant extension method

            findResultsArray = findResults as StubEntity[] ?? findResults.ToArray();
            Assert.False(findResultsArray.IsNullOrEmpty());
            Assert.True(findResultsArray.Length == 20);
        }

        [Fact]
        public async Task FindAllWithAndSpecification_Test()
        {
            // arrange
            var sut = this.repository;

            // act
            var findResults = await sut.FindAllAsync(
                new StubHasNameSpecification("Name1")
                .And(new StubHasTenantSpecification(this.tenantId))).ConfigureAwait(false);

            // assert
            var findResultsArray = findResults as StubEntity[] ?? findResults.ToArray();
            Assert.False(findResultsArray.IsNullOrEmpty());
            Assert.Equal("Name1", findResultsArray.FirstOrDefault()?.Name);
        }

        [Fact]
        public async Task FindAllWithOrSpecification_Test()
        {
            // arrange
            var sut = this.repository;

            // act
            var findResults = await sut.FindAllAsync(
                new StubHasNameSpecification("Name1")
                .Or(new StubHasNameSpecification("Name2"))).ConfigureAwait(false);

            // assert
            var findResultsArray = findResults as StubEntity[] ?? findResults.ToArray();
            Assert.False(findResultsArray.IsNullOrEmpty());
            Assert.True(findResultsArray.Count() == 2);
            Assert.Contains(findResultsArray, f => f.Name == "Name1");
            Assert.Contains(findResultsArray, f => f.Name == "Name2");
        }

        [Fact]
        public async Task FindAllWithNotSpecification_Test()
        {
            // arrange
            var sut = this.repository;

            // act
            var findResults = await sut.FindAllAsync(
                new StubHasTenantSpecification(this.tenantId)
                .And(new StubHasNameSpecification("Name1")
                     .Not())).ConfigureAwait(false);

            // assert
            var findResultsArray = findResults as StubEntity[] ?? findResults.ToArray();
            Assert.False(findResultsArray.IsNullOrEmpty());
            Assert.DoesNotContain(findResultsArray, f => f.Name == "Name1");
        }

        [Fact]
        public async Task FindAllWithSpecifications_Test()
        {
            // arrange
            var sut = this.repository;

            // act
            var findResults = await sut.FindAllAsync(
                new List<Specification<StubEntity>>
                {
                    new StubHasNameSpecification("Name1"),
                    new StubHasTenantSpecification(this.tenantId)
                }).ConfigureAwait(false);

            // assert
            var findResultsArray = findResults as StubEntity[] ?? findResults.ToArray();
            Assert.False(findResultsArray.IsNullOrEmpty());
            Assert.Equal("Name1", findResultsArray.FirstOrDefault()?.Name);

            findResults = await sut.FindAllAsync(
                new List<Specification<StubEntity>>
                {
                    new StubHasNameSpecification("Name1"),
                    new StubHasNameSpecification("Name2")
                }).ConfigureAwait(false);

            Assert.True(findResults.IsNullOrEmpty());
        }

        [Fact]
        public async Task Find_Test()
        {
            // arrange
            var sut = this.repository;

            // act
            var findResult = await sut.FindOneAsync("Id1").ConfigureAwait(false);

            // assert
            Assert.NotNull(findResult);
            Assert.Equal("Name1", findResult.Name);

            findResult = await sut.FindAsync("Id1", this.tenantId).ConfigureAwait(false);

            Assert.NotNull(findResult);
            Assert.Equal("Name1", findResult.Name);
        }

        [Fact]
        public async Task AddOrUpdate_Test()
        {
            // arrange
            var sut = this.repository;

            // act
            string[] regions = { "East", "West" };
            for (int i = 1; i < 21; i++)
            {
                var result = await sut.UpsertAsync(
                    new StubEntity
                    {
                        Id = $"Id{i}",
                        Name = $"Name{i}",
                        Region = regions[new Random().Next(0, regions.Length)],
                        TenantId = this.tenantId
                    }).ConfigureAwait(false);

                // assert
                Assert.NotNull(result);
            }

            await this.mediator.Received().Publish(Arg.Any<IDomainEvent>());
        }
    }

#pragma warning disable SA1402 // File may only contain a single class
    public class StubCosmosDbSqlRepository : CosmosDbSqlRepository<StubEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StubCosmosDbSqlRepository" /> class.
        /// </summary>
        /// <param name="mediator">The mediator.</param>
        /// <param name="provider">The provider.</param>
        public StubCosmosDbSqlRepository(IMediator mediator, ICosmosDbSqlProvider<StubEntity> provider)
            : base(mediator, provider)
        {
        }

        public async Task<StubEntity> GetByNameAsync(string orderNumber)
        {
            var entities = await this.FindAllAsync(new StubHasNameSpecification(orderNumber)).ConfigureAwait(false);
            return entities.FirstOrDefault();
        }
    }

    public class StubEntity : TenantEntity<string>, IAggregateRoot
    {
        public string Name { get; set; }

        public int Age { get; set; }

        public string Company { get; set; }

        public string Region { get; set; }
    }

    public class StubHasNameSpecification : Specification<StubEntity>
    {
        protected readonly string name;

        public StubHasNameSpecification(string name)
        {
            this.name = name;
        }

        public override Expression<Func<StubEntity, bool>> ToExpression()
        {
            return t => t.Name == this.name;
        }

        public static class Factory
        {
            public static StubHasNameSpecification Create(string name)
            {
                return new StubHasNameSpecification(name);
            }
        }
    }

    public class StubHasTenantSpecification : HasTenantSpecification<StubEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StubHasTenantSpecification"/> class.
        /// </summary>
        /// <param name="tenantId">The tenant identifier.</param>
        public StubHasTenantSpecification(string tenantId)
            : base(tenantId)
        {
        }
    }
#pragma warning restore SA1402 // File may only contain a single class
}
