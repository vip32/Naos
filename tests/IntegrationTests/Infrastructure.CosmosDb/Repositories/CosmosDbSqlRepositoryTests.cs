namespace Naos.Core.IntegrationTests.Infrastructure.CosmosDb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Naos.Core.App.Configuration;
    using Naos.Core.Common;
    using Naos.Core.Domain;
    using Naos.Core.Infrastructure.Azure.CosmosDb;
    using Xunit;

    public class CosmosDbSqlRepositoryTests
    {
        private readonly NaosConfiguration configuration = new NaosConfiguration();
        private readonly StubCosmosDbSqlRepository repository;
        private readonly string tenantId = "naos_test";

        public CosmosDbSqlRepositoryTests()
        {
            NaosConfigurationFactory.Bind(this.configuration);

            this.repository = new StubCosmosDbSqlRepository(
                new Mock<IMediator>().Object,
                new CosmosDbSqlProvider<StubEntity>(// kv >> development-naos--app--cosmosDb--serviceEndpointUri    development-naos--app--cosmosDb--authKeyOrResourceToken
                        client: CosmosDbClient.Create(this.configuration.App.CosmosDb.ServiceEndpointUri, this.configuration.App.CosmosDb.AuthKeyOrResourceToken),
                        databaseId: this.configuration.App.CosmosDb.DatabaseId,
                        collectionNameFactory: () => this.configuration.App.CosmosDb.CollectionName,
                        collectionOfferThroughput: this.configuration.App.CosmosDb.CollectionOfferThroughput,
                        collectionPartitionKey: this.configuration.App.CosmosDb.CollectionPartitionKey,
                        isMasterCollection: this.configuration.App.CosmosDb.IsMasterCollection));
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

            findResults = await sut.FindAllAsync(this.tenantId).ConfigureAwait(false);

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

            findResults = await sut.FindAllAsync(new HasTenantSpecification<StubEntity, string>(this.tenantId)).ConfigureAwait(false);

            findResultsArray = findResults as StubEntity[] ?? findResults.ToArray();
            Assert.False(findResultsArray.IsNullOrEmpty());
            Assert.True(findResultsArray.Length == 20);

            findResults = await sut.FindAllAsync(this.tenantId).ConfigureAwait(false); // tenant extension method

            findResultsArray = findResults as StubEntity[] ?? findResults.ToArray();
            Assert.False(findResultsArray.IsNullOrEmpty());
            Assert.True(findResultsArray.Length == 20);
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
        public async Task FindById_Test()
        {
            // arrange
            var sut = this.repository;

            // act
            var findResult = await sut.FindByIdAsync("Id1").ConfigureAwait(false);

            // assert
            Assert.NotNull(findResult);
            Assert.Equal("Name1", findResult.Name);

            findResult = await sut.FindByIdAsync("Id1", this.tenantId).ConfigureAwait(false);

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
                var result = await sut.AddOrUpdateAsync(new StubEntity
                {
                    Id = $"Id{i}",
                    Name = $"Name{i}",
                    Region = regions[new Random().Next(0, regions.Length)],
                    TenantId = this.tenantId
                }).ConfigureAwait(false);

                // assert
                Assert.NotNull(result);
            }
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

        public override Expression<Func<StubEntity, bool>> Expression()
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

    public class StubHasTenantSpecification : HasTenantSpecification<StubEntity, string>
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
