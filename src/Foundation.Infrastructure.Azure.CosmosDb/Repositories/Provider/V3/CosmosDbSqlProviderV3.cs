namespace Naos.Foundation.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net;
    using System.Threading.Tasks;
    using EnsureThat;
    using Humanizer;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Azure.Cosmos.Linq;
    using Microsoft.Extensions.Logging;

    // https://github.com/Azure/azure-cosmos-dotnet-v3
    // https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.cosmos
    public class CosmosDbSqlProviderV3<T> : ICosmosDbSqlProvider<T>, IDisposable
    //where T : IHaveDiscriminator // needed? each type T is persisted in own collection
    {
        private readonly CosmosDbSqlProviderV3Options<T> options;
        private readonly ILogger<CosmosDbSqlProviderV3<T>> logger;
        private CosmosClient client;
        private Database database;
        private Container container;
        private string containerName;

        public CosmosDbSqlProviderV3(
            CosmosDbSqlProviderV3Options<T> options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Client, nameof(options.Client));
            EnsureArg.IsNotNullOrEmpty(options.PartitionKey, nameof(options.PartitionKey));

            this.options = options;
            this.logger = this.options.CreateLogger<CosmosDbSqlProviderV3<T>>();
        }

        public CosmosDbSqlProviderV3(Builder<CosmosDbSqlProviderV3OptionsBuilder<T>, CosmosDbSqlProviderV3Options<T>> optionsBuilder)
            : this(optionsBuilder(new CosmosDbSqlProviderV3OptionsBuilder<T>()).Build())
        {
        }

        public async Task<T> GetByIdAsync(string id, object partitionKeyValue = null)
        {
            this.Initialize(this.options);
            var options = this.EnsureRequestOptions(partitionKeyValue);

            var sqlQuery = new QueryDefinition($"select * from {this.containerName} c where c.id = @id").WithParameter("@id", id);
            var iterator = this.container.GetItemQueryIterator<T>(
                sqlQuery,
                requestOptions: options);

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync().AnyContext();
                this.LogRequestCharge(response.RequestCharge, response.ActivityId);

                foreach (var result in response.Resource)
                {
                    return result;
                }
            }

            //try
            //{
            //    var response = await this.container.ReadItemAsync<T>(
            //    id,
            //    partitionKeyValue.IsNullOrEmpty() ? default : new PartitionKey(partitionKeyValue)).AnyContext();
            //    this.LogRequestCharge(response.RequestCharge, response.ActivityId);
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

        public async Task<T> UpsertAsync(T entity, object partitionKeyValue = null)
        {
            this.Initialize(this.options);
            var options = this.EnsureRequestOptions(partitionKeyValue);

            if (!options.PartitionKey.HasValue)
            {
                // Partition key value will be populated by extracting from {T}
                var response = await this.container.UpsertItemAsync(entity).AnyContext();
                this.LogRequestCharge(response.RequestCharge, response.ActivityId);
                return response.Resource;
            }
            else
            {
                var response = await this.container.UpsertItemAsync(
                    entity,
                    options.PartitionKey.Value).AnyContext();
                return response.Resource;
            }
        }

        public async Task<IEnumerable<T>> WhereAsync(
            Expression<Func<T, bool>> expression,
            int? skip = null,
            int? take = null,
            Expression<Func<T, object>> orderExpression = null,
            bool orderDescending = false,
            object partitionKeyValue = null)
        {
            this.Initialize(this.options);
            var options = this.EnsureRequestOptions(partitionKeyValue);

            double requestCharge = 0;
            var result = new List<T>();
            var iterator = this.container.GetItemLinqQueryable<T>(requestOptions: options)
                .WhereIf(expression)
                .SkipIf(skip)
                .TakeIf(take)
                .OrderByIf(orderExpression, orderDescending).ToFeedIterator(); // https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.cosmos.container.getitemlinqqueryable?view=azure-dotnet

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync().AnyContext();
                requestCharge += response.RequestCharge;
                foreach (var entity in response.Resource)
                {
                    result.Add(entity);
                }
            }

            this.LogRequestCharge(requestCharge);
            return result; // TODO: replace with IAsyncEnumerable (netstandard 2.1)
        }

        public async Task<IEnumerable<T>> WhereAsync(
            IEnumerable<Expression<Func<T, bool>>> expressions = null,
            int? skip = null,
            int? take = null,
            Expression<Func<T, object>> orderExpression = null,
            bool orderDescending = false,
            object partitionKeyValue = null)
        {
            this.Initialize(this.options);
            var options = this.EnsureRequestOptions(partitionKeyValue);

            double requestCharge = 0;
            var result = new List<T>();
            var iterator = this.container.GetItemLinqQueryable<T>(requestOptions: options)
                .WhereIf(expressions)
                .SkipIf(skip)
                .TakeIf(take)
                .OrderByIf(orderExpression, orderDescending).ToFeedIterator(); // https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.cosmos.container.getitemlinqqueryable?view=azure-dotnet

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync().AnyContext();
                requestCharge += response.RequestCharge;
                foreach (var entity in response.Resource)
                {
                    result.Add(entity);
                }
            }

            this.LogRequestCharge(requestCharge);
            return result; // TODO: replace with IAsyncEnumerable (netstandard 2.1)
        }

        public Task<IEnumerable<T>> WhereAsync( // OBSOLETE
            Expression<Func<T, bool>> expression,
            Expression<Func<T, T>> selector,
            int? skip = null,
            int? take = null,
            Expression<Func<T, object>> orderExpression = null,
            bool orderDescending = false,
            object partitionKeyValue = null)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteByIdAsync(string id, object partitionKeyValue = null)
        {
            this.Initialize(this.options);
            var options = this.EnsureRequestOptions(partitionKeyValue);

            var entity = await this.GetByIdAsync(id, partitionKeyValue).AnyContext();
            if(entity == null)
            {
                return false;
            }

            try
            {
                var partitionKey = PartitionKey.Null;
                if(partitionKeyValue == null)
                {
                    if(this.options.PartitionKeyStringExpression != null)
                    {
                        partitionKey = new PartitionKey(this.options.PartitionKeyStringExpression.Invoke(entity));
                    }
                    else if (this.options.PartitionKeyBoolExpression != null)
                    {
                        partitionKey = new PartitionKey(this.options.PartitionKeyBoolExpression.Invoke(entity));
                    }
                    else if (this.options.PartitionKeyDoubleExpression != null)
                    {
                        partitionKey = new PartitionKey(this.options.PartitionKeyDoubleExpression.Invoke(entity));
                    }
                }
                else
                {
                    partitionKey = this.EnsureRequestOptions(partitionKeyValue).PartitionKey.Value;
                }

                var response = await this.container.DeleteItemAsync<T>(
                    id,
                    partitionKey).AnyContext();
                this.LogRequestCharge(response.RequestCharge, response.ActivityId);

                return response.StatusCode == HttpStatusCode.NoContent;
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

        private QueryRequestOptions EnsureRequestOptions(object partitionKeyValue)
        {
            var options = new QueryRequestOptions();
            if (partitionKeyValue != null)
            {
                options.PartitionKey = partitionKeyValue switch
                {
                    string s => new PartitionKey(s),
                    bool b => new PartitionKey(b),
                    double d => new PartitionKey(d),
                    _ => throw new ArgumentException(
                            message: "unsupported partition key value type (string, bool, double)",
                            paramName: nameof(partitionKeyValue)),
                };
            }

            return options;
        }

        private void Initialize(CosmosDbSqlProviderV3Options<T> options)
        {
            if (this.container == null)
            {
                this.client = options.Client;
                this.database = /*await */this.client
                    .CreateDatabaseIfNotExistsAsync(options.Database.EmptyToNull() ?? "master", throughput: options.ThroughPut).Result;
                this.containerName = options.Container.EmptyToNull() ?? typeof(T).PrettyName().Pluralize().ToLower();
                this.container = /*await*/this.database
                    .CreateContainerIfNotExistsAsync(
                        new ContainerProperties(
                            this.containerName,
                            partitionKeyPath: this.options.PartitionKey)
                    // TODO: set timetolive (ttl)
                    {
                        //IndexingPolicy = new Microsoft.Azure.Cosmos.IndexingPolicy(new RangeIndex(Microsoft.Azure.Cosmos.DataType.String) { Precision = -1 })
                    },
                        throughput: options.ThroughPut).Result;
            }
        }

        private void LogRequestCharge(double requestCharge, string activityId = null)
        {
            this.logger.LogDebug($"cosmos request charge: {requestCharge} (instance={this.database.Id}.{this.container.Id}, activityId={activityId})");
        }

        //private void LogRequestCharge(IEnumerable<double> requestCharges, IEnumerable<string> activityIds)
        //{
        //    this.logger.LogDebug($"cosmos request charge total: {requestCharges.Sum()} (instance={this.database.Id}.{this.container.Id}, activityId=multiple)");

        //    //_logger.LogInformation($"cosmos request charge: {this.database.Id}.{this.container.Id};  Total RC: {requestCharges.Sum()}");
        //    //_logger.LogInformation($"cosmos request charge: detail: ActiveIds: {activityIds.ToString(", ")}");
        //    //_logger.LogInformation($"cosmos request charge: detail: requestCharges: {requestCharges.ToString(", ")}");
        //}
    }
}
