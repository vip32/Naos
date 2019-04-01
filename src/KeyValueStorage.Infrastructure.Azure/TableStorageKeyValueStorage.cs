namespace Naos.Core.KeyValueStorage.Infrastructure.Azure
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.Table;
    using Naos.Core.Common;
    using Naos.Core.KeyValueStorage.Domain;

    public class TableStorageKeyValueStorage : IKeyValueStorage
    {
        private const int MaxInsertLimit = 100;
        private readonly TableStorageKeyValueStorageOptions options;
        private readonly ILogger<TableStorageKeyValueStorage> logger;
        private readonly CloudTableClient client;
        private readonly ConcurrentDictionary<string, TableInfo> tableInfos = new ConcurrentDictionary<string, TableInfo>();

        public TableStorageKeyValueStorage(TableStorageKeyValueStorageOptions options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNullOrEmpty(options.AccountName, nameof(options.AccountName));
            EnsureArg.IsNotNullOrEmpty(options.StorageKey, nameof(options.StorageKey));

            this.options = options;
            this.logger = options.CreateLogger<TableStorageKeyValueStorage>();

            var account = new CloudStorageAccount(new StorageCredentials(options.AccountName, options.StorageKey), true);
            this.client = account.CreateCloudTableClient();
        }

        public TableStorageKeyValueStorage(Builder<TableStorageKeyValueStorageOptionsBuilder, TableStorageKeyValueStorageOptions> optionsBuilder)
            : this(optionsBuilder(new TableStorageKeyValueStorageOptionsBuilder()).Build())
        {
        }

        public async Task<IEnumerable<string>> GetTableNames()
        {
            var result = new List<string>();
            TableContinuationToken token = null;
            do
            {
                var tables = await this.client.ListTablesSegmentedAsync(token);
                foreach (var table in tables.Results)
                {
                    result.Add(table.Name);
                }

                token = tables.ContinuationToken;
            }
            while (token != null);

            return result;
        }

        public async Task<bool> DeleteAsync(string tableName)
        {
            var table = await this.GetTableAsync(tableName, false);
            if (table != null)
            {
                await table.DeleteAsync();
                return this.tableInfos.TryRemove(tableName, out TableInfo tag);
            }

            return false;
        }

        public async Task<IEnumerable<Value>> GetAsync(string tableName, Key key)
        {
            EnsureArg.IsNotNullOrEmpty(tableName, nameof(tableName));
            EnsureArg.IsNotNull(key, nameof(key));

            return await this.InternalGetAsync(tableName, key, -1);
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
               (b, te) => b.Insert(te),
               rowsList);
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
               (b, te) => b.InsertOrReplace(te),
               rowsList);
        }

        public async Task UpdateAsync(string tableName, IEnumerable<Value> values)
        {
            await this.BatchedOperationAsync(tableName, false,
               (b, te) => b.Replace(te),
               values);
        }

        public async Task MergeAsync(string tableName, IEnumerable<Value> values)
        {
            await this.BatchedOperationAsync(tableName, true,
               (b, te) => b.InsertOrMerge(te),
               values);
        }

        public async Task DeleteAsync(string tableName, IEnumerable<Key> rowIds)
        {
            if (rowIds == null)
            {
                return;
            }

            await this.BatchedOperationAsync(tableName, true,
               (b, te) => b.Delete(te),
               rowIds);
        }

        public void Dispose()
        {
            // nothing to dispose
        }

        private async Task BatchedOperationAsync(
            string tableName,
            bool createTable,
            Action<TableBatchOperation, ITableEntity> action,
            IEnumerable<Value> rows)
        {
            EnsureArg.IsNotNullOrEmpty(tableName, nameof(tableName));
            EnsureArg.IsNotNull(rows, nameof(rows));

            var table = await this.GetTableAsync(tableName, createTable);
            if (table == null)
            {
                return;
            }

            await Task.WhenAll(rows.GroupBy(e => e.PartitionKey).Select(g => this.BatchedOperationAsync(table, g, action)));
        }

        private async Task BatchedOperationAsync(CloudTable table, IGrouping<string, Value> group, Action<TableBatchOperation, ITableEntity> action)
        {
            foreach (IEnumerable<Value> chunk in group.Chunk(MaxInsertLimit))
            {
                if (chunk == null)
                {
                    break;
                }

                var chunks = new List<Value>(chunk);
                var batch = new TableBatchOperation();
                foreach (Value row in chunks)
                {
                    action(batch, new EntityAdapter(row));
                }

                var result = await this.ExecuteBatchAsync(table, batch);
                for (int i = 0; i < result.Count && i < chunks.Count; i++)
                {
                    var tableResult = result[i];
                    var row = chunks[i];
                }
            }
        }

        private async Task BatchedOperationAsync(string tableName, bool createTable,
           Action<TableBatchOperation, ITableEntity> action,
           IEnumerable<Key> rowIds)
        {
            EnsureArg.IsNotNullOrEmpty(tableName, nameof(tableName));

            if (rowIds == null)
            {
                return;
            }

            var table = await this.GetTableAsync(tableName, createTable);
            if (table == null)
            {
                return;
            }

            foreach (IGrouping<string, Key> group in rowIds.GroupBy(e => e.PartitionKey))
            {
                foreach (IEnumerable<Key> chunk in group.Chunk(MaxInsertLimit))
                {
                    if (chunk == null)
                    {
                        break;
                    }

                    var batch = new TableBatchOperation();
                    foreach (var row in chunk)
                    {
                        action(batch, new EntityAdapter(row));
                    }

                    await this.ExecuteBatchAsync(table, batch);
                }
            }
        }

        private async Task<CloudTable> GetTableAsync(string tableName, bool createIfNotExists)
        {
            EnsureArg.IsNotNullOrEmpty(tableName, nameof(tableName));

            if (!new Regex("^[A-Za-z][A-Za-z0-9]{2,62}$").IsMatch(tableName))
            {
                throw new ArgumentException($"invalid table name: {tableName}", nameof(tableName));
            }

            var cached = this.tableInfos.TryGetValue(tableName, out TableInfo tag);
            if (!cached)
            {
                tag = new TableInfo
                {
                    Table = this.client.GetTableReference(tableName),
                };
                tag.Exists = await tag.Table.ExistsAsync();
                this.tableInfos[tableName] = tag;
            }

            if (!tag.Exists && createIfNotExists)
            {
                await tag.Table.CreateAsync();
                tag.Exists = true;
            }

            if (!tag.Exists)
            {
                return null;
            }

            return tag.Table;
        }

        private async Task<List<TableResult>> ExecuteBatchAsync(CloudTable table, TableBatchOperation operation)
        {
            try
            {
                return (await table.ExecuteBatchAsync(operation)).ToList();
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

        private async Task<IEnumerable<Value>> InternalGetAsync(string tableName, Key key, int take)
        {
            var table = await this.GetTableAsync(tableName, false);
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
                   this.EncodeKey(key.PartitionKey)));
            }

            if (key?.RowKey != null)
            {
                filters.Add(TableQuery.GenerateFilterCondition(
                   "RowKey",
                   QueryComparisons.Equal,
                   this.EncodeKey(key.RowKey)));
            }

            var query = new TableQuery();
            if (filters.Count > 0)
            {
                string filter = filters[0];
                for (int i = 1; i < filters.Count; i++)
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
                var queryResults = await table.ExecuteQuerySegmentedAsync(query, token);
                entities.AddRange(queryResults.Results);
                token = queryResults.ContinuationToken;
            }
            while (token != null);

            return entities.Select(this.ToValue).ToList();
        }

        private string EncodeKey(string key)
        {
            //todo: read more: https://msdn.microsoft.com/library/azure/dd179338.aspx
            return WebUtility.UrlEncode(key);
        }

        private class TableInfo
        {
            public CloudTable Table { get; set; }

            public bool Exists { get; set; }
        }
    }
}
