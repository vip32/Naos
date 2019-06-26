namespace Naos.Core.KeyValueStorage.Infrastructure.Azure
{
    using Naos.Foundation;

    public class TableKeyValueStorageOptions : BaseOptions
    {
        public string ConnectionString { get; set; }

        public int MaxInsertLimit { get; set; } = 100;
    }
}
