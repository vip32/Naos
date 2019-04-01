namespace Naos.Core.KeyValueStorage.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using EnsureThat;
    using Humanizer;
    using Naos.Core.Common;
    using Naos.Core.KeyValueStorage;

    public static class Extensions
    {
        public static async Task<IEnumerable<T>> GetAsync<T>(this IKeyValueStorage source, string partitionKey, string rowKey)
            where T : class, new()
        {
            return await source.GetAsync<T>(new Key(partitionKey, rowKey)).AnyContext();
        }

        public static async Task<IEnumerable<T>> GetAsync<T>(this IKeyValueStorage source, Key key)
            where T : class, new()
        {
            var results = await source.GetAsync(typeof(T).Name.Pluralize(), key).AnyContext();
            return results.Select(r =>
            {
                var instance = r.ToObject<T>();
                MapKey(r, instance);
                return instance;
            });
        }

        public static async Task<T> GetOneAsync<T>(this IKeyValueStorage source, string partitionKey, string rowKey)
            where T : class, new()
        {
            return await source.GetOneAsync<T>(new Key(partitionKey, rowKey)).AnyContext();
        }

        public static async Task<T> GetOneAsync<T>(this IKeyValueStorage source, Key key)
            where T : class, new()
        {
            var result = await source.GetOneAsync(typeof(T).Name.Pluralize(), key).AnyContext();

            var instance = result.ToObject<T>();
            MapKey(result, instance);
            return instance;
        }

        public static async Task InsertAsync<T>(this IKeyValueStorage source, IEnumerable<T> values)
            where T : class, new()
        {
            await source.InsertAsync(typeof(T).Name.Pluralize(), values.Safe().Select(Map)).AnyContext();
        }

        public static async Task InsertAsync<T>(this IKeyValueStorage source, T value)
            where T : class, new()
        {
            await source.InsertAsync(typeof(T).Name.Pluralize(), Map(value)).AnyContext();
        }

        public static async Task UpsertAsync<T>(this IKeyValueStorage source, T value)
            where T : class, new()
        {
            await source.UpsertAsync(typeof(T).Name.Pluralize(), Map(value)).AnyContext();
        }

        public static async Task UpsertAsync<T>(this IKeyValueStorage source, IEnumerable<T> values)
            where T : class, new()
        {
            await source.UpsertAsync(typeof(T).Name.Pluralize(), values.Safe().Select(Map)).AnyContext();
        }

        public static async Task UpdateAsync<T>(this IKeyValueStorage source, T value)
            where T : class, new()
        {
            await source.UpdateAsync(typeof(T).Name.Pluralize(), Map(value)).AnyContext();
        }

        public static async Task UpdateAsync<T>(this IKeyValueStorage source, IEnumerable<T> values)
            where T : class, new()
        {
            await source.UpdateAsync(typeof(T).Name.Pluralize(), values.Safe().Select(Map)).AnyContext();
        }

        public static async Task InsertAsync(this IKeyValueStorage source, string tableName, Value value)
        {
            EnsureArg.IsNotNullOrEmpty(tableName, nameof(tableName));
            EnsureArg.IsNotNull(value, nameof(value));

            await source.InsertAsync(new List<Value> { value }).AnyContext();
        }

        public static async Task UpsertAsync(this IKeyValueStorage source, string tableName, Value value)
        {
            EnsureArg.IsNotNullOrEmpty(tableName, nameof(tableName));
            EnsureArg.IsNotNull(value, nameof(value));

            await source.UpsertAsync(new List<Value> { value }).AnyContext();
        }

        public static async Task UpdateAsync(this IKeyValueStorage source, string tableName, Value value)
        {
            EnsureArg.IsNotNullOrEmpty(tableName, nameof(tableName));
            EnsureArg.IsNotNull(value, nameof(value));

            await source.UpdateAsync(new List<Value> { value }).AnyContext();
        }

        /// <summary>
        /// Gets first value by partitionkey and rowkey
        /// </summary>
        /// <param name="source"></param>
        /// <param name="tableName"></param>
        /// <param name="partitionKey"></param>
        /// <param name="rowKey"></param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Value> GetOneAsync(this IKeyValueStorage source, string tableName, string partitionKey, string rowKey)
        {
            EnsureArg.IsNotNullOrEmpty(partitionKey, nameof(partitionKey));
            EnsureArg.IsNotNullOrEmpty(rowKey, nameof(rowKey));

            return await source.GetOneAsync(tableName, new Key(partitionKey, rowKey)).AnyContext();
        }

        /// <summary>
        /// Gets first value by key
        /// </summary>
        /// <param name="source"></param>
        /// <param name="tableName"></param>
        /// <param name="key"></param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Value> GetOneAsync(this IKeyValueStorage source, string tableName, Key key)
        {
            EnsureArg.IsNotNull(key, nameof(key));

            var values = await source.GetAsync(tableName, key).AnyContext();
            return values.FirstOrDefault();
        }

        /// <summary>
        /// Deletes record by key
        /// </summary>
        /// <param name="source"></param>
        /// <param name="tableName"></param>
        /// <param name="key"></param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static Task DeleteAsync(this IKeyValueStorage source, string tableName, Key key)
        {
            return source.DeleteAsync(tableName, new[] { key });
        }

        private static Value Map<T>(T value)
            where T : class, new()
        {
            var values = value.ToDictionary().Safe();
            values.TryGetValue("PartitionKey", out object partitionKey);
            values.TryGetValue("RowKey", out object rowKey);

            var result = new Value(new Key(partitionKey?.ToString(), rowKey?.ToString()));
            values.ForEach(vd => result.Add(vd.Key, vd.Value));
            return result;
        }

        private static void MapKey<T>(Value value, T instance)
            where T : class, new()
        {
            var partitionKeyProperty = typeof(T).GetProperty("PartitionKey", BindingFlags.Public | BindingFlags.Instance);
            if (partitionKeyProperty?.CanWrite == true)
            {
                partitionKeyProperty.SetValue(instance, value.PartitionKey, null);
            }

            var rowKeyProperty = typeof(T).GetProperty("RowKey", BindingFlags.Public | BindingFlags.Instance);
            if (rowKeyProperty?.CanWrite == true)
            {
                rowKeyProperty.SetValue(instance, value.RowKey, null);
            }
        }
    }
}
