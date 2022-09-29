namespace Naos.KeyValueStorage.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using EnsureThat;
    using LightningDB;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.KeyValueStorage.Domain;

    /// <summary>
    /// A table keyvalue provider for file storage.
    /// </summary>
    public class LightningDbKeyValueStorage : IKeyValueStorage
    {
        private readonly LightningDbKeyValueStorageOptions options;
        private readonly ILogger<LightningDbKeyValueStorage> logger;
        private readonly LightningEnvironment env;

        public LightningDbKeyValueStorage(LightningDbKeyValueStorageOptions options)
        {
            EnsureArg.IsNotNull(options, nameof(options));

            this.options = options;
            this.options = options;
            this.options.Folder = options.Folder.EmptyToNull() ?? Path.GetTempPath();

            this.logger = options.CreateLogger<LightningDbKeyValueStorage>();
            this.env = new LightningEnvironment(this.options.Folder);
            this.env.MaxDatabases = 2;
            this.env.Open();
        }

        public LightningDbKeyValueStorage(Builder<LightningDbKeyValueStorageOptionsBuilder, LightningDbKeyValueStorageOptions> optionsBuilder)
            : this(optionsBuilder(new LightningDbKeyValueStorageOptionsBuilder()).Build())
        {
        }

        public Task<IEnumerable<string>> GetTableNames()
        {
            //var result = new List<string>();
            //TableContinuationToken token = null;
            //do
            //{
            //    var tables = await this.client.ListTablesSegmentedAsync(token);
            //    foreach(var table in tables.Results)
            //    {
            //        result.Add(table.Name);
            //    }

            //    token = tables.ContinuationToken;
            //}
            //while(token != null);

            //return result;

            throw new NotSupportedException();
        }

        public Task<bool> DeleteTableAsync(string tableName)
        {
            //var table = await this.EnsureTableAsync(tableName, false);
            //if(table != null)
            //{
            //    await table.DeleteAsync();
            //    return this.tableInfos.TryRemove(tableName, out var tag);
            //}

            //return false;
            throw new NotSupportedException();
        }

        public Task<IEnumerable<Value>> FindAllAsync(string tableName, Key key)
        {
            EnsureArg.IsNotNullOrEmpty(tableName, nameof(tableName));
            EnsureArg.IsNotNull(key, nameof(key));

            using (var tx = this.env.BeginTransaction())
            using (var db = tx.OpenDatabase(tableName, new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create }))
            {
                var value = tx.Get(db, Encoding.UTF8.GetBytes($"{key.PartitionKey}-{key.RowKey}"));
                var result = SerializationHelper.BsonByteDeserialize<Value>(value.value.CopyToNewArray());
                result.PartitionKey = key.PartitionKey;
                result.RowKey = key.RowKey;

                return Task.FromResult(new[] { result }.AsEnumerable());
            }
        }

        public Task<IEnumerable<Value>> FindAllAsync(string tableName, IEnumerable<Criteria> criterias)
        {
            EnsureArg.IsNotNullOrEmpty(tableName, nameof(tableName));

            throw new NotSupportedException();
        }

        public Task InsertAsync(string tableName, IEnumerable<Value> values)
        {
            EnsureArg.IsNotNullOrEmpty(tableName, nameof(tableName));

            using (var tx = this.env.BeginTransaction())
            using (var db = tx.OpenDatabase(tableName, new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create }))
            {
                foreach (var value in values?.Where(v => v.Key != null))
                {
                    tx.Put(db, Encoding.UTF8.GetBytes($"{value.Key.PartitionKey}-{value.Key.RowKey}"), SerializationHelper.BsonByteSerialize(value), PutOptions.NoDuplicateData);
                }

                tx.Commit();
            }

            return Task.CompletedTask;
        }

        public Task UpsertAsync(string tableName, IEnumerable<Value> values)
        {
            EnsureArg.IsNotNullOrEmpty(tableName, nameof(tableName));

            using (var tx = this.env.BeginTransaction())
            using (var db = tx.OpenDatabase(tableName, new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create }))
            {
                foreach (var value in values?.Where(v => v.Key != null))
                {
                    tx.Put(db, Encoding.UTF8.GetBytes($"{value.Key.PartitionKey}-{value.Key.RowKey}"), SerializationHelper.BsonByteSerialize(value), PutOptions.NoDuplicateData);
                }

                tx.Commit();
            }

            return Task.CompletedTask;
        }

        public Task UpdateAsync(string tableName, IEnumerable<Value> values)
        {
            EnsureArg.IsNotNullOrEmpty(tableName, nameof(tableName));

            using (var tx = this.env.BeginTransaction())
            using (var db = tx.OpenDatabase(tableName, new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create }))
            {
                foreach (var value in values?.Where(v => v.Key != null))
                {
                    tx.Put(db, Encoding.UTF8.GetBytes($"{value.Key.PartitionKey}-{value.Key.RowKey}"), SerializationHelper.BsonByteSerialize(value), PutOptions.NoDuplicateData);
                }

                tx.Commit();
            }

            return Task.CompletedTask;
        }

        public Task MergeAsync(string tableName, IEnumerable<Value> values)
        {
            EnsureArg.IsNotNullOrEmpty(tableName, nameof(tableName));

            using (var tx = this.env.BeginTransaction())
            using (var db = tx.OpenDatabase(tableName, new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create }))
            {
                foreach (var value in values?.Where(v => v.Key != null))
                {
                    tx.Put(db, Encoding.UTF8.GetBytes($"{value.Key.PartitionKey}-{value.Key.RowKey}"), SerializationHelper.BsonByteSerialize(value), PutOptions.NoDuplicateData);
                }

                tx.Commit();
            }

            return Task.CompletedTask;
        }

        public Task DeleteAsync(string tableName, IEnumerable<Key> keys)
        {
            EnsureArg.IsNotNullOrEmpty(tableName, nameof(tableName));

            using (var tx = this.env.BeginTransaction())
            using (var db = tx.OpenDatabase(tableName, new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create }))
            {
                foreach (var key in keys)
                {
                    tx.Delete(db, Encoding.UTF8.GetBytes($"{key.PartitionKey}-{key.RowKey}"), null);
                }

                tx.Commit();
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            this.env?.Dispose();
        }
    }
}
