namespace Naos.KeyValueStorage.Infrastructure
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.KeyValueStorage.Domain;

    /// <summary>
    /// A table keyvalue provider for Azure Table Storage AND Azure Cosmos DB Table API.
    /// One provider for both backends made possible by the universal client (Microsoft.Azure.Cosmos.Table)
    /// Which backend to use depends on the connectionstring used.
    /// </summary>
    public class TableKeyValueStorage : IKeyValueStorage
    {
        // https://docs.microsoft.com/en-us/azure/cosmos-db/tutorial-develop-table-dotnet
        private readonly TableKeyValueStorageOptions options;
        private readonly ILogger<TableKeyValueStorage> logger;
        private readonly CloudTableClient client;
        private readonly ConcurrentDictionary<string, TableInfo> tableInfos = new ConcurrentDictionary<string, TableInfo>();
        private bool isCosmos;

        public TableKeyValueStorage(TableKeyValueStorageOptions options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNullOrEmpty(options.ConnectionString, nameof(options.ConnectionString));

            this.options = options;
            this.logger = options.CreateLogger<TableKeyValueStorage>();

            var account = CloudStorageAccount.Parse(this.options.ConnectionString);
            this.client = account.CreateCloudTableClient();

            if (this.options.ConnectionString.Contains("table.cosmos.azure.com", StringComparison.OrdinalIgnoreCase))
            {
                this.isCosmos = true;
            }
        }

        public TableKeyValueStorage(Builder<TableKeyValueStorageOptionsBuilder, TableKeyValueStorageOptions> optionsBuilder)
            : this(optionsBuilder(new TableKeyValueStorageOptionsBuilder()).Build())
        {
        }

        public async Task<IEnumerable<string>> GetTableNames()
        {
            var result = new List<string>();
            TableContinuationToken token = null;
            do
            {
                var tables = await this.client.ListTablesSegmentedAsync(token).AnyContext();
                foreach (var table in tables.Results)
                {
                    result.Add(table.Name);
                }

                token = tables.ContinuationToken;
            }
            while (token != null);

            return result;
        }

        public async Task<bool> DeleteTableAsync(string tableName)
        {
            var table = await this.EnsureTableAsync(tableName, false).AnyContext();
            if (table != null)
            {
                await table.DeleteAsync().AnyContext();
                return this.tableInfos.TryRemove(tableName, out var tag);
            }

            return false;
        }

        public async Task<IEnumerable<Value>> FindAllAsync(string tableName, Key key)
        {
            EnsureArg.IsNotNullOrEmpty(tableName, nameof(tableName));
            EnsureArg.IsNotNull(key, nameof(key));

            return await this.FindAsync(tableName, key).AnyContext();
        }

        public async Task<IEnumerable<Value>> FindAllAsync(string tableName, IEnumerable<Criteria> criterias)
        {
            EnsureArg.IsNotNullOrEmpty(tableName, nameof(tableName));

            if (!this.isCosmos)
            {
                // criteria cause table scan when Azure Table Storage is the backend
                //throw new NotSupportedException();
            }

            return await this.FindAsync(tableName, criterias: criterias).AnyContext();
        }

        public async Task InsertAsync(string tableName, IEnumerable<Value> values)
        {
            EnsureArg.IsNotNullOrEmpty(tableName, nameof(tableName));
            EnsureArg.IsNotNull(values, nameof(values));

            var rowsList = values.ToList();
            if (rowsList.Count == 0)
            {
                return;
            }

            if (!Value.AreDistinct(rowsList))
            {
                throw new StorageException("DuplicateKey", null);
            }

            await this.BatchedOperationAsync(tableName, true,
               (op, te) => op.Insert(te),
               rowsList).AnyContext();
        }

        public async Task UpsertAsync(string tableName, IEnumerable<Value> values)
        {
            EnsureArg.IsNotNullOrEmpty(tableName, nameof(tableName));
            EnsureArg.IsNotNull(values, nameof(values));

            var rowsList = values.ToList();
            if (rowsList.Count == 0)
            {
                return;
            }

            if (!Value.AreDistinct(rowsList))
            {
                throw new StorageException("DuplicateKey", null);
            }

            await this.BatchedOperationAsync(tableName, true,
               (op, te) => op.InsertOrReplace(te),
               rowsList).AnyContext();
        }

        public async Task UpdateAsync(string tableName, IEnumerable<Value> values)
        {
            await this.BatchedOperationAsync(tableName, false,
               (op, te) => op.Replace(te),
               values).AnyContext();
        }

        public async Task MergeAsync(string tableName, IEnumerable<Value> values)
        {
            await this.BatchedOperationAsync(tableName, true,
               (op, te) => op.InsertOrMerge(te),
               values).AnyContext();
        }

        public async Task DeleteAsync(string tableName, IEnumerable<Key> keys)
        {
            if (keys == null)
            {
                return;
            }

            await this.BatchedOperationAsync(tableName, true,
               (op, te) => op.Delete(te),
               keys).AnyContext();
        }

        public void Dispose()
        {
            // nothing to dispose
        }

        private async Task BatchedOperationAsync(
            string tableName,
            bool createTable,
            Action<TableBatchOperation, ITableEntity> action,
            IEnumerable<Value> values)
        {
            EnsureArg.IsNotNullOrEmpty(tableName, nameof(tableName));
            EnsureArg.IsNotNull(values, nameof(values));

            var table = await this.EnsureTableAsync(tableName, createTable).AnyContext();
            if (table == null)
            {
                return;
            }

            await Task.WhenAll(values.GroupBy(e => e.PartitionKey).Select(g => this.BatchedOperationAsync(table, g, action))).AnyContext();
        }

        private async Task BatchedOperationAsync(
            CloudTable table,
            IGrouping<string, Value> valueGroups,
            Action<TableBatchOperation, ITableEntity> action)
        {
            foreach (var valuesChunk in valueGroups.Chunk(this.options.MaxInsertLimit))
            {
                if (valuesChunk == null)
                {
                    break;
                }

                var values = new List<Value>(valuesChunk);
                var batch = new TableBatchOperation();
                foreach (var value in values)
                {
                    action(batch, new EntityAdapter(
                        value,
                        this.isCosmos ? new[] { "Id", "Timestamp" } : null));
                }

                var result = await this.ExecuteBatchAsync(table, batch).AnyContext();
                for (var i = 0; i < result.Count && i < values.Count; i++)
                {
                    var tableResult = result[i];
                    var value = values[i];
                }
            }
        }

        private async Task BatchedOperationAsync(string tableName, bool createTable,
           Action<TableBatchOperation, ITableEntity> action,
           IEnumerable<Key> keys)
        {
            EnsureArg.IsNotNullOrEmpty(tableName, nameof(tableName));

            if (keys == null)
            {
                return;
            }

            var table = await this.EnsureTableAsync(tableName, createTable).AnyContext();
            if (table == null)
            {
                return;
            }

            foreach (var group in keys.GroupBy(e => e.PartitionKey))
            {
                foreach (var chunk in group.Chunk(this.options.MaxInsertLimit))
                {
                    if (chunk == null)
                    {
                        break;
                    }

                    var batch = new TableBatchOperation();
                    foreach (var key in chunk)
                    {
                        action(batch, new EntityAdapter(
                            key,
                            this.isCosmos ? new[] { "Id", "Timestamp" } : null));
                    }

                    await this.ExecuteBatchAsync(table, batch).AnyContext();
                }
            }
        }

        private async Task<CloudTable> EnsureTableAsync(string tableName, bool createIfNotExists)
        {
            EnsureArg.IsNotNullOrEmpty(tableName, nameof(tableName));

            if (!new Regex("^[A-Za-z][A-Za-z0-9]{2,62}$").IsMatch(tableName))
            {
                throw new ArgumentException($"table name {tableName} not valid", nameof(tableName));
            }

            var cached = this.tableInfos.TryGetValue(tableName, out var info);
            if (!cached)
            {
                info = new TableInfo
                {
                    Table = this.client.GetTableReference(tableName),
                };
                info.Exists = await info.Table.ExistsAsync().AnyContext();
                this.tableInfos[tableName] = info;
            }

            if (!info.Exists && createIfNotExists)
            {
                await info.Table.CreateAsync().AnyContext(); // WARN: CAS issue https://github.com/Azure/azure-cosmos-table-dotnet/issues/7
                //Thread.Sleep(1500);
                info.Exists = true;
            }

            if (!info.Exists)
            {
                return null;
            }

            return info.Table;
        }

        private async Task<List<TableResult>> ExecuteBatchAsync(CloudTable table, TableBatchOperation operation)
        {
            try
            {
                return (await table.ExecuteBatchAsync(operation).AnyContext()).ToList();
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == 409)
                {
                    throw new StorageException("DuplicateKey", ex);
                }

                throw;
            }
        }

        private Value ToValue(DynamicTableEntity entity)
        {
            var result = new Value(entity.PartitionKey, entity.RowKey);
            foreach (var property in entity.Properties)
            {
                switch (property.Value.PropertyType)
                {
                    case EdmType.Boolean:
                        result[property.Key] = property.Value.BooleanValue;
                        break;
                    case EdmType.DateTime:
                        result[property.Key] = property.Value.DateTime.Value.ToUniversalTime();
                        break;
                    case EdmType.Int32:
                        result[property.Key] = property.Value.Int32Value;
                        break;
                    case EdmType.Int64:
                        result[property.Key] = property.Value.Int64Value;
                        break;
                    case EdmType.Double:
                        result[property.Key] = property.Value.DoubleValue;
                        break;
                    case EdmType.Guid:
                        result[property.Key] = property.Value.GuidValue;
                        break;
                    case EdmType.Binary:
                        result[property.Key] = property.Value.BinaryValue;
                        break;
                    default:
                        result[property.Key] = property.Value.StringValue;
                        break;
                }
            }

            return result;
        }

        private async Task<IEnumerable<Value>> FindAsync(
            string tableName,
            Key key = null,
            int take = -1,
            IEnumerable<Criteria> criterias = null)
        {
            var table = await this.EnsureTableAsync(tableName, false).AnyContext();
            if (table == null)
            {
                return new List<Value>();
            }

            var filters = new List<string>();
            if (key?.PartitionKey != null)
            {
                filters.Add(TableQuery.GenerateFilterCondition(
                   "PartitionKey",
                   QueryComparisons.Equal,
                   WebUtility.UrlEncode(key.PartitionKey)));
            }

            if (key?.RowKey != null)
            {
                filters.Add(TableQuery.GenerateFilterCondition(
                   "RowKey",
                   QueryComparisons.Equal,
                   WebUtility.UrlEncode(key.RowKey)));
            }

            this.Map(criterias, filters);

            var query = new TableQuery();
            if (filters.Count > 0)
            {
                var filter = filters[0];
                for (var i = 1; i < filters.Count; i++)
                {
                    filter = TableQuery.CombineFilters(filter, TableOperators.And, filters[i]);
                }

                query = query.Where(filter);
            }

            if (take > 0)
            {
                query = query.Take(take);
            }

            TableContinuationToken token = null;
            var entities = new List<DynamicTableEntity>();
            do
            {
                var queryResults = await table.ExecuteQuerySegmentedAsync(query, token).AnyContext();
                entities.AddRange(queryResults.Results);
                token = queryResults.ContinuationToken;
            }
            while (token != null);

            return entities.Select(this.ToValue).ToList();
        }

        private void Map(IEnumerable<Criteria> criterias, List<string> filters)
        {
            foreach (var criteria in criterias.Safe())
            {
                if (criteria.Value is int i)
                {
                    filters.Add(TableQuery.GenerateFilterConditionForInt(
                       criteria.Name,
                       criteria.Operator.ToAbbreviation(),
                       i));
                }
                else if (criteria.Value is double d)
                {
                    filters.Add(TableQuery.GenerateFilterConditionForDouble(
                       criteria.Name,
                       criteria.Operator.ToAbbreviation(),
                       d));
                }
                else if (criteria.Value is long l)
                {
                    filters.Add(TableQuery.GenerateFilterConditionForLong(
                       criteria.Name,
                       criteria.Operator.ToAbbreviation(),
                       l));
                }
                else if (criteria.Value is bool b)
                {
                    filters.Add(TableQuery.GenerateFilterConditionForBool(
                       criteria.Name,
                       criteria.Operator.ToAbbreviation(),
                       b));
                }
                else if (criteria.Value is DateTime dt)
                {
                    filters.Add(TableQuery.GenerateFilterConditionForDate(
                       criteria.Name,
                       criteria.Operator.ToAbbreviation(),
                       dt));
                }
                else
                {
                    filters.Add(TableQuery.GenerateFilterCondition(
                       criteria.Name,
                       criteria.Operator.ToAbbreviation(),
                       WebUtility.UrlEncode(criteria.Value.As<string>())));
                }
            }
        }

        private class TableInfo
        {
            public CloudTable Table { get; set; }

            public bool Exists { get; set; }
        }
    }
}
