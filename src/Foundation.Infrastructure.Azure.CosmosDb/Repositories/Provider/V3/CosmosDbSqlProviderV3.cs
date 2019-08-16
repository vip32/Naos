namespace Naos.Foundation.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using EnsureThat;
    using Humanizer;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Azure.Cosmos.Linq;
    using Naos.Foundation.Domain;

    public class CosmosDbSqlProviderV3<T> : ICosmosDbSqlProvider<T>, IDisposable
        where T : IHaveDiscriminator // needed? each type T is persisted in own collection
    {
        private readonly CosmosClient client;
        private readonly string partitionKeyPath;
        private readonly string partitionKeyValue;
        private readonly Database database;
        private readonly Container container;

        public CosmosDbSqlProviderV3(CosmosDbSqlProviderV3Options options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Client, nameof(options.Client));

            // https://azure.microsoft.com/en-us/blog/azure-cosmos-dotnet-sdk-version-3-0-now-in-public-preview/
            // https://github.com/Azure/azure-cosmos-dotnet-v3
            // https://github.com/Azure/azure-cosmos-dotnet-v3/issues/68
            this.client = options.Client;
            this.partitionKeyPath = options.PartitionKeyPath.EmptyToNull() ?? "/Discriminator"; // needed? each type T is persisted in own collection
            this.partitionKeyValue = typeof(T).FullName;

            this.database = /*await */this.client
                .CreateDatabaseIfNotExistsAsync(options.Database.EmptyToNull() ?? "master", throughput: options.ThroughPut).Result;
            this.container = /*await*/this.database
                .CreateContainerIfNotExistsAsync(
                    new ContainerProperties(
                        options.Container.EmptyToNull() ?? typeof(T).PrettyName().Pluralize().ToLower(),
                        partitionKeyPath: this.partitionKeyPath)
                    {
                        //IndexingPolicy = new IndexingPolicy(new RangeIndex(DataType.String) { Precision = -1 })
                    },
                    throughput: options.ThroughPut).Result;
        }

        public CosmosDbSqlProviderV3(Builder<CosmosDbSqlProviderV3OptionsBuilder, CosmosDbSqlProviderV3Options> optionsBuilder)
            : this(optionsBuilder(new CosmosDbSqlProviderV3OptionsBuilder()).Build())
        {
        }

        public async Task<T> GetByIdAsync(string id, string partitionKey = null) // partitionkey
        {
            var response = await this.container.ReadItemAsync<T>(
                id,
                new PartitionKey(partitionKey ?? this.partitionKeyValue)).AnyContext();
            return response.Resource;
        }

        public async Task<T> UpsertAsync(T entity, string partitionKey = null)
        {
            var response = await this.container.UpsertItemAsync(
                entity,
                new PartitionKey(partitionKey ?? this.partitionKeyValue)).AnyContext();
            return response.Resource;
        }

        public async Task<IEnumerable<T>> WhereAsync(
            Expression<Func<T, bool>> expression,
            string partitionKey = null,
            int? skip = null,
            int? take = null,
            Expression<Func<T, object>> orderExpression = null,
            bool orderDescending = false)
        {
            var result = new List<T>();
            var iterator = this.container.GetItemLinqQueryable<T>(
                requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(partitionKey ?? this.partitionKeyValue) })
                .WhereIf(expression)
                .SkipIf(skip)
                .TakeIf(take)
                .OrderByIf(orderExpression, orderDescending).ToFeedIterator(); // https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.cosmos.container.getitemlinqqueryable?view=azure-dotnet

            while (iterator.HasMoreResults)
            {
                foreach (var item in await iterator.ReadNextAsync())
                {
                    result.Add(item);
                }
            }

            return result; // TODO: replace with IAsyncEnumerable (netstandard 2.1)
        }

        public async Task<IEnumerable<T>> WhereAsync(
            IEnumerable<Expression<Func<T, bool>>> expressions = null,
            string partitionKey = null,
            int? skip = null,
            int? take = null,
            Expression<Func<T, object>> orderExpression = null,
            bool orderDescending = false)
        {
            var result = new List<T>();
            var iterator = this.container.GetItemLinqQueryable<T>(
                requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(partitionKey ?? this.partitionKeyValue) })
                .WhereIf(expressions)
                .SkipIf(skip)
                .TakeIf(take)
                .OrderByIf(orderExpression, orderDescending).ToFeedIterator(); // https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.cosmos.container.getitemlinqqueryable?view=azure-dotnet

            while (iterator.HasMoreResults)
            {
                foreach (var item in await iterator.ReadNextAsync())
                {
                    result.Add(item);
                }
            }

            return result; // TODO: replace with IAsyncEnumerable (netstandard 2.1)
        }

        public Task<IEnumerable<T>> WhereAsync( // OBSOLETE
            Expression<Func<T, bool>> expression,
            Expression<Func<T, T>> selector,
            string partitionKey = null,
            int? skip = null,
            int? take = null,
            Expression<Func<T, object>> orderExpression = null,
            bool orderDescending = false)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteByIdAsync(string id, string partitionKey = null)
        {
            var response = await this.container.DeleteItemAsync<T>(id, new PartitionKey(partitionKey ?? this.partitionKeyValue)).AnyContext();
            return true; // TODO
        }

        public void Dispose()
        {
            this.client?.Dispose();
        }
    }
}
