namespace Naos.Sample.App.IntegrationTests.Infrastructure.Azure.CosmosDb
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.Extensions.Logging;
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
                Substitute.For<ILogger<StubCosmosDbSqlRepository>>(),
                this.mediator,
                new CosmosDbSqlProvider<StubEntity>(
                        client: CosmosDbClient.Create(AppConfiguration.CosmosDb.ServiceEndpointUri, AppConfiguration.CosmosDb.AuthKeyOrResourceToken),
                        databaseId: AppConfiguration.CosmosDb.DatabaseId,
                        collectionIdFactory: () => AppConfiguration.CosmosDb.CollectionId,
                        collectionPartitionKey: AppConfiguration.CosmosDb.CollectionPartitionKey,
                        collectionOfferThroughput: AppConfiguration.CosmosDb.CollectionOfferThroughput,
                        isMasterCollection: AppConfiguration.CosmosDb.IsMasterCollection));
        }

        [Fact]
        public async Task FindOne_Test()
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
    }

#pragma warning disable SA1402 // File may only contain a single class
    public class StubCosmosDbSqlRepository : CosmosDbSqlRepository<StubEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StubCosmosDbSqlRepository" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="mediator">The mediator.</param>
        /// <param name="provider">The provider.</param>
        public StubCosmosDbSqlRepository(ILogger<StubCosmosDbSqlRepository> logger, IMediator mediator, ICosmosDbSqlProvider<StubEntity> provider)
            : base(logger, mediator, provider)
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
