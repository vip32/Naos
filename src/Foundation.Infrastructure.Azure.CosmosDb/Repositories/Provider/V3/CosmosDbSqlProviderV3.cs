namespace Naos.Foundation.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Net;
    using System.Threading.Tasks;
    using EnsureThat;
    using Humanizer;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Azure.Cosmos.Linq;

    public class CosmosDbSqlProviderV3<T> : ICosmosDbSqlProvider<T>, IDisposable
    //where T : IHaveDiscriminator // needed? each type T is persisted in own collection
    {
        private readonly CosmosClient client;
        private readonly string partitionKeyPath;
        private readonly Database database;
        private readonly string containerName;
        private readonly Container container;

        public CosmosDbSqlProviderV3(CosmosDbSqlProviderV3Options options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Client, nameof(options.Client));
            EnsureArg.IsNotNullOrEmpty(options.PartitionKeyPath, nameof(options.PartitionKeyPath));

            // https://github.com/Azure/azure-cosmos-dotnet-v3
            // https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.cosmos
            this.client = options.Client;
            this.partitionKeyPath = options.PartitionKeyPath;

            // TODO: make below lazy
            this.database = /*await */this.client
                .CreateDatabaseIfNotExistsAsync(options.Database.EmptyToNull() ?? "master", throughput: options.ThroughPut).Result;
            this.containerName = options.Container.EmptyToNull() ?? typeof(T).PrettyName().Pluralize().ToLower();
            this.container = /*await*/this.database
                .CreateContainerIfNotExistsAsync(
                    new ContainerProperties(
                        this.containerName,
                        partitionKeyPath: this.partitionKeyPath)
                        // TODO: set timetolive (ttl)
                    {
                        //IndexingPolicy = new Microsoft.Azure.Cosmos.IndexingPolicy(new RangeIndex(Microsoft.Azure.Cosmos.DataType.String) { Precision = -1 })
                    },
                    throughput: options.ThroughPut).Result;
        }

        public CosmosDbSqlProviderV3(Builder<CosmosDbSqlProviderV3OptionsBuilder, CosmosDbSqlProviderV3Options> optionsBuilder)
            : this(optionsBuilder(new CosmosDbSqlProviderV3OptionsBuilder()).Build())
        {
        }

        public async Task<T> GetByIdAsync(string id, string partitionKeyValue = null)
        {
            var sqlQuery = new QueryDefinition($"select * from {this.containerName} c where c.id = @id").WithParameter("@id", id);
            var options = new QueryRequestOptions();
            if (!partitionKeyValue.IsNullOrEmpty())
            {
                options.PartitionKey = new PartitionKey(partitionKeyValue);
            }

            var iterator = this.container.GetItemQueryIterator<T>(
                sqlQuery,
                requestOptions: options);

            while (iterator.HasMoreResults)
            {
                foreach (var item in await iterator.ReadNextAsync())
                {
                    {
                        return item;
                    }
                }
            }

            //try
            //{
            //    var response = await this.container.ReadItemAsync<T>(
            //    id,
            //    partitionKeyValue.IsNullOrEmpty() ? default : new PartitionKey(partitionKeyValue)).AnyContext();
            //    return response.Resource;
            //}
            //catch (CosmosException ex)
            //{
            //    if (ex.StatusCode == HttpStatusCode.NotFound)
            //    {
            //        return default;
            //    }

            //    throw;
            //}

            return default;
        }

        public async Task<T> UpsertAsync(T entity, string partitionKeyValue = null)
        {
            if (partitionKeyValue.IsNullOrEmpty())
            {
                // Partition key value will be populated by extracting from {T}
                var response = await this.container.UpsertItemAsync(entity).AnyContext();
                return response.Resource;
            }
            else
            {
                var response = await this.container.UpsertItemAsync(
                    entity,
                    new PartitionKey(partitionKeyValue)).AnyContext();
                return response.Resource;
            }
        }

        public async Task<IEnumerable<T>> WhereAsync(
            Expression<Func<T, bool>> expression,
            int? skip = null,
            int? take = null,
            Expression<Func<T, object>> orderExpression = null,
            bool orderDescending = false,
            string partitionKeyValue = null)
        {
            var result = new List<T>();
            var options = new QueryRequestOptions();
            if (!partitionKeyValue.IsNullOrEmpty())
            {
                options.PartitionKey = new PartitionKey(partitionKeyValue);
            }

            var iterator = this.container.GetItemLinqQueryable<T>(requestOptions: options)
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
            string partitionKeyValue = null,
            int? skip = null,
            int? take = null,
            Expression<Func<T, object>> orderExpression = null,
            bool orderDescending = false)
        {
            var result = new List<T>();
            var options = new QueryRequestOptions();
            if (!partitionKeyValue.IsNullOrEmpty())
            {
                options.PartitionKey = new PartitionKey(partitionKeyValue);
            }

            var iterator = this.container.GetItemLinqQueryable<T>(requestOptions: options)
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
            int? skip = null,
            int? take = null,
            Expression<Func<T, object>> orderExpression = null,
            bool orderDescending = false,
            string partitionKeyValue = null)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteByIdAsync(string id, string partitionKeyValue = null)
        {
            try
            {
                // partitionKeyValue workaround (if not provided): otherwhise document is not found
                // var item = GetByIdAsync(id)
                // get partition value from item by using this.partitionKeyPath (json selector?)
                // use this partition value below (DeleteItemAsync)

                var response = await this.container.DeleteItemAsync<T>(
                    id,
                    partitionKeyValue.IsNullOrEmpty() ? PartitionKey.Null : new PartitionKey(partitionKeyValue)).AnyContext();
                return true;
                // TODO: evaulate response.StatusCode == HttpStatusCode.OK
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    return false;
                }

                throw;
            }
        }

        public void Dispose()
        {
            this.client?.Dispose();
        }
    }
}
