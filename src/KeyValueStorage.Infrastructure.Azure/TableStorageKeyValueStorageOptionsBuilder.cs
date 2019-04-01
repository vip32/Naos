namespace Naos.Core.KeyValueStorage.Infrastructure.Azure
{
    using Naos.Core.Common;

    public class TableStorageKeyValueStorageOptionsBuilder :
        BaseOptionsBuilder<TableStorageKeyValueStorageOptions, TableStorageKeyValueStorageOptionsBuilder>
    {
        public TableStorageKeyValueStorageOptionsBuilder AccountName(string accountName)
        {
            this.Target.AccountName = accountName;
            return this;
        }

        public TableStorageKeyValueStorageOptionsBuilder StorageKey(string storageKey)
        {
            this.Target.StorageKey = storageKey;

            return this;
        }
    }
}