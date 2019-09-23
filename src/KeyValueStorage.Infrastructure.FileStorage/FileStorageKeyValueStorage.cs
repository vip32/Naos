namespace Naos.KeyValueStorage.Infrastructure.FileStorage
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.FileStorage.Domain;
    using Naos.Foundation;
    using Naos.KeyValueStorage.Domain;

    /// <summary>
    /// A table keyvalue provider for file storage.
    /// </summary>
    public class FileStorageKeyValueStorage : IKeyValueStorage
    {
        private readonly FileStorageKeyValueStorageOptions options;
        private readonly ILogger<FileStorageKeyValueStorage> logger;

        public FileStorageKeyValueStorage(FileStorageKeyValueStorageOptions options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.FileStorage, nameof(options.FileStorage));

            this.options = options;
            this.logger = options.CreateLogger<FileStorageKeyValueStorage>();
            // ....\temp\data\naos_keyvaultstorage\TABLENAME\PARTITIONKEY\ROWKEY.json
        }

        public FileStorageKeyValueStorage(Builder<FileStorageKeyValueStorageOptionsBuilder, FileStorageKeyValueStorageOptions> optionsBuilder)
            : this(optionsBuilder(new FileStorageKeyValueStorageOptionsBuilder()).Build())
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

        public async Task<IEnumerable<Value>> FindAllAsync(string tableName, Key key)
        {
            EnsureArg.IsNotNullOrEmpty(tableName, nameof(tableName));
            EnsureArg.IsNotNull(key, nameof(key));

            return await this.FindAsync(tableName, key).AnyContext();
        }

        public async Task<IEnumerable<Value>> FindAllAsync(string tableName, IEnumerable<Criteria> criterias)
        {
            EnsureArg.IsNotNullOrEmpty(tableName, nameof(tableName));

            return await this.FindAsync(tableName, criterias: criterias).AnyContext();
        }

        public async Task InsertAsync(string tableName, IEnumerable<Value> values)
        {
            EnsureArg.IsNotNullOrEmpty(tableName, nameof(tableName));

            foreach (var value in values?.Where(v => v.Key != null))
            {
                await this.options.FileStorage.SaveFileObjectAsync(
                    Path.Combine(tableName, $"{value.Key.PartitionKey}--{value.Key.RowKey}"), value).AnyContext();
            }
        }

        public async Task UpsertAsync(string tableName, IEnumerable<Value> values)
        {
            EnsureArg.IsNotNullOrEmpty(tableName, nameof(tableName));

            foreach (var value in values?.Where(v => v.Key != null))
            {
                await this.options.FileStorage.SaveFileObjectAsync(
                    Path.Combine(tableName, $"{value.Key.PartitionKey}--{value.Key.RowKey}"), value).AnyContext();
            }
        }

        public async Task UpdateAsync(string tableName, IEnumerable<Value> values)
        {
            EnsureArg.IsNotNullOrEmpty(tableName, nameof(tableName));

            foreach (var value in values?.Where(v => v.Key != null))
            {
                await this.options.FileStorage.SaveFileObjectAsync(
                    Path.Combine(tableName, $"{value.Key.PartitionKey}--{value.Key.RowKey}"), value).AnyContext();
            }
        }

        public async Task MergeAsync(string tableName, IEnumerable<Value> values)
        {
            EnsureArg.IsNotNullOrEmpty(tableName, nameof(tableName));

            foreach (var value in values?.Where(v => v.Key != null))
            {
                await this.options.FileStorage.SaveFileObjectAsync(
                    Path.Combine(tableName, $"{value.Key.PartitionKey}--{value.Key.RowKey}"), value).AnyContext();
            }
        }

        public async Task DeleteAsync(string tableName, IEnumerable<Key> keys)
        {
            EnsureArg.IsNotNullOrEmpty(tableName, nameof(tableName));

            foreach (var key in keys)
            {
                await this.options.FileStorage.DeleteFileAsync(
                    Path.Combine(tableName, $"{key.PartitionKey}--{key.RowKey}")).AnyContext();
            }
        }

        public void Dispose()
        {
            this.options.FileStorage?.Dispose();
        }

        private async Task<IEnumerable<Value>> FindAsync(
            string tableName,
            Key key = null,
            int take = -1,
            IEnumerable<Criteria> criterias = null)
        {
            var searchPattern = Path.Combine(tableName.Safe(), "*");

            if (key?.PartitionKey != null)
            {
                searchPattern = Path.Combine(tableName.Safe(), key?.PartitionKey.Safe(), "*");
            }

            if (key?.PartitionKey != null && key?.RowKey != null)
            {
                searchPattern = Path.Combine(tableName.Safe(), $"{key?.PartitionKey}--{key?.RowKey}*");
            }

            var result = new List<Value>();
            var fileInformations = await this.options.FileStorage.GetFileInformationsAsync(searchPattern).AnyContext();
            foreach (var fileInformation in fileInformations.Safe())
            {
                var value = await this.options.FileStorage.GetFileObjectAsync<Value>(fileInformation.Path).AnyContext();
                if (value != null && this.Match(criterias, value)) // in memory criteria matching
                {
                    if (key != null)
                    {
                        value.PartitionKey = key.PartitionKey;
                        value.RowKey = key.RowKey;
                    }

                    result.Add(value);
                }
            }

            return result;
        }

        private bool Match(IEnumerable<Criteria> criterias, Value value)
        {
            var result = true;
            foreach (var criteria in criterias.Safe())
            {
                if (criteria.Value is string s && value[criteria.Name] is string sv)
                {
                    if (criteria.Operator == CriteriaOperator.Equal)
                    {
                        result = sv == s;
                    }
                    else if (criteria.Operator == CriteriaOperator.NotEqual)
                    {
                        result = sv != s;
                    }

                    //else if(criteria.Operator == CriteriaOperator.GreaterThan)
                    //{
                    //    result = sv > s;
                    //}
                    //else if(criteria.Operator == CriteriaOperator.GreaterThanOrEqual)
                    //{
                    //    result = sv >= s;
                    //}
                    //else if(criteria.Operator == CriteriaOperator.LessThan)
                    //{
                    //    result = sv < s;
                    //}
                    //else if(criteria.Operator == CriteriaOperator.LessThanOrEqual)
                    //{
                    //    result = sv <= s;
                    //}
                }
                else if (criteria.Value is int i && value[criteria.Name] is int iv)
                {
                    if (criteria.Operator == CriteriaOperator.Equal)
                    {
                        result = iv == i;
                    }
                    else if (criteria.Operator == CriteriaOperator.NotEqual)
                    {
                        result = iv != i;
                    }
                    else if (criteria.Operator == CriteriaOperator.GreaterThan)
                    {
                        result = iv > i;
                    }
                    else if (criteria.Operator == CriteriaOperator.GreaterThanOrEqual)
                    {
                        result = iv >= i;
                    }
                    else if (criteria.Operator == CriteriaOperator.LessThan)
                    {
                        result = iv < i;
                    }
                    else if (criteria.Operator == CriteriaOperator.LessThanOrEqual)
                    {
                        result = iv <= i;
                    }
                }
                else if (criteria.Value is double d && value[criteria.Name] is double dv)
                {
                    if (criteria.Operator == CriteriaOperator.Equal)
                    {
                        result = dv == d;
                    }
                    else if (criteria.Operator == CriteriaOperator.NotEqual)
                    {
                        result = dv != d;
                    }
                    else if (criteria.Operator == CriteriaOperator.GreaterThan)
                    {
                        result = dv > d;
                    }
                    else if (criteria.Operator == CriteriaOperator.GreaterThanOrEqual)
                    {
                        result = dv >= d;
                    }
                    else if (criteria.Operator == CriteriaOperator.LessThan)
                    {
                        result = dv < d;
                    }
                    else if (criteria.Operator == CriteriaOperator.LessThanOrEqual)
                    {
                        result = dv <= d;
                    }
                }
                else if (criteria.Value is long l && value[criteria.Name] is long lv)
                {
                    if (criteria.Operator == CriteriaOperator.Equal)
                    {
                        result = lv == l;
                    }
                    else if (criteria.Operator == CriteriaOperator.NotEqual)
                    {
                        result = lv != l;
                    }
                    else if (criteria.Operator == CriteriaOperator.GreaterThan)
                    {
                        result = lv > l;
                    }
                    else if (criteria.Operator == CriteriaOperator.GreaterThanOrEqual)
                    {
                        result = lv >= l;
                    }
                    else if (criteria.Operator == CriteriaOperator.LessThan)
                    {
                        result = lv < l;
                    }
                    else if (criteria.Operator == CriteriaOperator.LessThanOrEqual)
                    {
                        result = lv <= l;
                    }
                }
                else if (criteria.Value is bool b && value[criteria.Name.Decapitalize()] is bool bv)
                {
                    if (criteria.Operator == CriteriaOperator.Equal)
                    {
                        result = bv == b;
                    }
                    else if (criteria.Operator == CriteriaOperator.NotEqual)
                    {
                        result = bv != b;
                    }
                }

                if (!result)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
