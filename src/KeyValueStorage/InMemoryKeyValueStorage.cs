namespace Naos.KeyValueStorage
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.KeyValueStorage.Domain;

    public class InMemoryKeyValueStorage : IKeyValueStorage
    {
        private readonly InMemoryKeyValueStorageOptions options;
        private readonly ILogger<InMemoryKeyValueStorage> logger;

        public InMemoryKeyValueStorage(InMemoryKeyValueStorageOptions options)
        {
            EnsureArg.IsNotNull(options, nameof(options));

            this.options = options;
            this.logger = options.CreateLogger<InMemoryKeyValueStorage>();
        }

        public InMemoryKeyValueStorage(Builder<InMemoryKeyValueStorageOptionsBuilder, InMemoryKeyValueStorageOptions> optionsBuilder)
            : this(optionsBuilder(new InMemoryKeyValueStorageOptionsBuilder()).Build())
        {
        }

        public Task DeleteAsync(string tableName, IEnumerable<Key> keys)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> DeleteTableAsync(string tableName)
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<Value>> FindAllAsync(string tableName, Key key)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<Value>> FindAllAsync(string tableName, IEnumerable<Criteria> criterias)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<string>> GetTableNames()
        {
            throw new System.NotImplementedException();
        }

        public Task InsertAsync(string tableName, IEnumerable<Value> values)
        {
            throw new System.NotImplementedException();
        }

        public Task MergeAsync(string tableName, IEnumerable<Value> values)
        {
            throw new System.NotImplementedException();
        }

        public Task UpdateAsync(string tableName, IEnumerable<Value> values)
        {
            throw new System.NotImplementedException();
        }

        public Task UpsertAsync(string tableName, IEnumerable<Value> values)
        {
            throw new System.NotImplementedException();
        }
    }
}