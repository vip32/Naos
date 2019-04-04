namespace Naos.Core.Infrastructure.Azure.CosmosDb
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using EnsureThat;
    using Humanizer;
    using Microsoft.Azure.Cosmos;
    using Naos.Core.Common;
    using Naos.Core.Domain;

    public class CosmosDbSqlProviderV3<T> : ICosmosDbSqlProvider<T>, IDisposable
        where T : IHaveDiscriminator // needed? each type T is persisted in own collection
    {
        private readonly CosmosClient client;
        private readonly string partitionKeyPath;
        private readonly string partitionKeyValue;
        private readonly CosmosDatabase database;
        private readonly CosmosContainer container;

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

            this.database = /*await */this.client.Databases
                .CreateDatabaseIfNotExistsAsync(options.Database.EmptyToNull() ?? "master", throughput: options.ThroughPut).Result;
            this.container = /*await*/this.database.Containers
                .CreateContainerIfNotExistsAsync(
                    new CosmosContainerSettings(
                        options.Container.EmptyToNull() ?? typeof(T).PrettyName().Pluralize().ToLower(),
                        partitionKeyPath: this.partitionKeyPath)
                        {
                            IndexingPolicy = new IndexingPolicy(new RangeIndex(DataType.String) { Precision = -1 })
                        },
                        throughput: options.ThroughPut).Result;
        }

        public CosmosDbSqlProviderV3(Builder<CosmosDbSqlProviderV3OptionsBuilder, CosmosDbSqlProviderV3Options> optionsBuilder)
            : this(optionsBuilder(new CosmosDbSqlProviderV3OptionsBuilder()).Build())
        {
        }

        public async Task<T> GetByIdAsync(string id)
        {
            var response = await this.container.Items.ReadItemAsync<T>(
                this.partitionKeyValue,
                id).AnyContext();
            return response.Resource;
        }

        public async Task<T> UpsertAsync(T entity)
        {
            var response = await this.container.Items.UpsertItemAsync(
                this.partitionKeyValue,
                entity).AnyContext();
            return response.Resource;
        }

        public /*async*/ Task<IEnumerable<T>> WhereAsync(Expression<Func<T, bool>> expression)
        {
            // linq not supported yet https://github.com/Azure/azure-cosmos-dotnet-v3/issues/4
            //var response = await this.container.Items.CreateItemQuery()
            //return response.Resource;
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> WhereAsync(Expression<Func<T, bool>> expression = null, IEnumerable<Expression<Func<T, bool>>> expressions = null, int count = 100, Expression<Func<T, object>> orderExpression = null, bool orderDescending = false)
        {
            // linq not supported yet https://github.com/Azure/azure-cosmos-dotnet-v3/issues/4
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> WhereAsync(Expression<Func<T, bool>> expression, Expression<Func<T, T>> selector, int count = 100, Expression<Func<T, object>> orderExpression = null, bool orderDescending = false)
        {
            // linq not supported yet https://github.com/Azure/azure-cosmos-dotnet-v3/issues/4
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteByIdAsync(string id)
        {
            var response = await this.container.Items.DeleteItemAsync<T>(this.partitionKeyValue, id);
            return true; // TODO
        }

        public void Dispose()
        {
            this.client?.Dispose();
        }
    }
}
