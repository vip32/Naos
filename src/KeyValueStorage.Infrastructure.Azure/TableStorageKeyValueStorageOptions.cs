namespace Naos.Core.KeyValueStorage.Infrastructure.Azure
{
    using Naos.Core.Common;

    public class TableStorageKeyValueStorageOptions : BaseOptions
    {
        public string AccountName { get; set; }

        public string StorageKey { get; set; }
    }
}
