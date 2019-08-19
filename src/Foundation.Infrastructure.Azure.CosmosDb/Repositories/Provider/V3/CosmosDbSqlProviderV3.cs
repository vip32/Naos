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

    // https://github.com/Azure/azure-cosmos-dotnet-v3
    // https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.cosmos
    public class CosmosDbSqlProviderV3<T> : ICosmosDbSqlProvider<T>, IDisposable
    //where T : IHaveDiscriminator // needed? each type T is persisted in own collection
    {
        private readonly CosmosDbSqlProviderV3Options options;
        private readonly Func<T, string> partitionKeyStringExpression;
        private readonly Func<T, bool> partitionKeyBoolExpression;
        private readonly Func<T, double> partitionKeyDoubleExpression;
        private readonly string partitionKey;
        private CosmosClient client;
        private Database database;
        private Container container;
        private string containerName;

        public CosmosDbSqlProviderV3(Builder<CosmosDbSqlProviderV3OptionsBuilder, CosmosDbSqlProviderV3Options> optionsBuilder, Expression<Func<T, string>> partitionKeyExpression)
            : this(optionsBuilder(new CosmosDbSqlProviderV3OptionsBuilder()).Build(), partitionKeyExpression)
        {
        }

        public CosmosDbSqlProviderV3(
            CosmosDbSqlProviderV3Options options,
            Expression<Func<T, string>> partitionKeyExpression = null) // TODO: ^^ move to options? is mandatory however
            : this(options, partitionKeyExpression, null, null)
        {
        }

        public CosmosDbSqlProviderV3(
            CosmosDbSqlProviderV3Options options,
            Expression<Func<T, bool>> partitionKeyExpression = null) // TODO: ^^ move to options? is mandatory however
            : this(options, null, partitionKeyExpression, null)
        {
        }

        public CosmosDbSqlProviderV3(
            CosmosDbSqlProviderV3Options options,
            Expression<Func<T, double>> partitionKeyExpression = null) // TODO: ^^ move to options? is mandatory however
            : this(options, null, null, partitionKeyExpression)
        {
        }

        internal CosmosDbSqlProviderV3(
            CosmosDbSqlProviderV3Options options,
            Expression<Func<T, string>> partitionKeyStringExpression = null,
            Expression<Func<T, bool>> partitionKeyBoolExpression = null,
            Expression<Func<T, double>> partitionKeyDoubleExpression = null) // TODO: ^^ move to options? is mandatory however
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Client, nameof(options.Client));
            //EnsureArg.IsNotNullOrEmpty(options.PartitionKey, nameof(options.PartitionKey));

            this.options = options;

            if (options.PartitionKey.IsNullOrEmpty())
            {
                // partitionkey name based on provided expression
                if (partitionKeyStringExpression != null)
                {
                    this.partitionKeyStringExpression = partitionKeyStringExpression.Compile();
                    this.partitionKey = $"/{partitionKeyStringExpression.ToExpressionString().Replace(".", "/")}";
                }
                else if (partitionKeyBoolExpression != null)
                {
                    this.partitionKeyBoolExpression = partitionKeyBoolExpression.Compile();
                    this.partitionKey = $"/{partitionKeyBoolExpression.ToExpressionString().Replace(".", "/")}";
                }
                else if (partitionKeyDoubleExpression != null)
                {
                    this.partitionKeyDoubleExpression = partitionKeyDoubleExpression.Compile();
                    this.partitionKey = $"/{partitionKeyDoubleExpression.ToExpressionString().Replace(".", "/")}";
                }
                else
                {
                    // implicit mode /_partitionKey, based on documenttype
                }
            }
            else
            {
                // provided partitionkey name (string)
                this.partitionKey = options.PartitionKey;
            }
        }

        public async Task<T> GetByIdAsync(string id, object partitionKeyValue = null)
        {
            this.Initialize(this.options);
            var options = this.EnsureOptions(partitionKeyValue);

            var sqlQuery = new QueryDefinition($"select * from {this.containerName} c where c.id = @id").WithParameter("@id", id);
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

        public async Task<T> UpsertAsync(T entity, object partitionKeyValue = null)
        {
            this.Initialize(this.options);
            var options = this.EnsureOptions(partitionKeyValue);

            if (partitionKeyValue == null)
            {
                // Partition key value will be populated by extracting from {T}
                var response = await this.container.UpsertItemAsync(entity).AnyContext();
                return response.Resource;
            }
            else
            {
                var response = await this.container.UpsertItemAsync(
                    entity,
                    this.EnsureOptions(partitionKeyValue).PartitionKey.Value).AnyContext();
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
            var options = this.EnsureOptions(partitionKeyValue);

            var result = new List<T>();
            var iterator = this.container.GetItemLinqQueryable<T>(requestOptions: options)
                .WhereIf(expression)
                .SkipIf(skip)
                .TakeIf(take)
                .OrderByIf(orderExpression, orderDescending).ToFeedIterator(); // https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.cosmos.container.getitemlinqqueryable?view=azure-dotnet

            while (iterator.HasMoreResults)
            {
                foreach (var entity in await iterator.ReadNextAsync())
                {
                    result.Add(entity);
                }
            }

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
            var options = this.EnsureOptions(partitionKeyValue);

            var result = new List<T>();
            var iterator = this.container.GetItemLinqQueryable<T>(requestOptions: options)
                .WhereIf(expressions)
                .SkipIf(skip)
                .TakeIf(take)
                .OrderByIf(orderExpression, orderDescending).ToFeedIterator(); // https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.cosmos.container.getitemlinqqueryable?view=azure-dotnet

            while (iterator.HasMoreResults)
            {
                foreach (var entity in await iterator.ReadNextAsync())
                {
                    result.Add(entity);
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
            object partitionKeyValue = null)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteByIdAsync(string id, object partitionKeyValue = null)
        {
            this.Initialize(this.options);
            var options = this.EnsureOptions(partitionKeyValue);

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
                    if(this.partitionKeyStringExpression != null)
                    {
                        partitionKey = new PartitionKey(this.partitionKeyStringExpression.Invoke(entity));
                    }
                    else if (this.partitionKeyBoolExpression != null)
                    {
                        partitionKey = new PartitionKey(this.partitionKeyBoolExpression.Invoke(entity));
                    }
                    else if (this.partitionKeyDoubleExpression != null)
                    {
                        partitionKey = new PartitionKey(this.partitionKeyDoubleExpression.Invoke(entity));
                    }
                }
                else
                {
                    partitionKey = this.EnsureOptions(partitionKeyValue).PartitionKey.Value;
                }

                var response = await this.container.DeleteItemAsync<T>(
                    id,
                    partitionKey).AnyContext();

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

        private QueryRequestOptions EnsureOptions(object partitionKeyValue)
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

        private void Initialize(CosmosDbSqlProviderV3Options options)
        {
            this.client = options.Client;
            this.database = /*await */this.client
                .CreateDatabaseIfNotExistsAsync(options.Database.EmptyToNull() ?? "master", throughput: options.ThroughPut).Result;
            this.containerName = options.Container.EmptyToNull() ?? typeof(T).PrettyName().Pluralize().ToLower();
            this.container = /*await*/this.database
                .CreateContainerIfNotExistsAsync(
                    new ContainerProperties(
                        this.containerName,
                        partitionKeyPath: this.partitionKey)
                    // TODO: set timetolive (ttl)
                    {
                        //IndexingPolicy = new Microsoft.Azure.Cosmos.IndexingPolicy(new RangeIndex(Microsoft.Azure.Cosmos.DataType.String) { Precision = -1 })
                    },
                    throughput: options.ThroughPut).Result;
        }
    }
}
