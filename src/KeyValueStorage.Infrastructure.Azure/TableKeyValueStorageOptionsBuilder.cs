namespace Naos.Core.KeyValueStorage.Infrastructure.Azure
{
    using Naos.Core.Common;

    public class TableKeyValueStorageOptionsBuilder :
        BaseOptionsBuilder<TableKeyValueStorageOptions, TableKeyValueStorageOptionsBuilder>
    {
        public TableKeyValueStorageOptionsBuilder ConnectionString(string connectionString)
        {
            this.Target.ConnectionString = connectionString;
            return this;
        }

        public TableKeyValueStorageOptionsBuilder MaxInsertLimit(int value)
        {
            this.Target.MaxInsertLimit = value;
            return this;
        }
    }
}