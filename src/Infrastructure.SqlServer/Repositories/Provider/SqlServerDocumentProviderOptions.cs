namespace Naos.Foundation.Infrastructure
{
    using Humanizer;

    public class SqlServerDocumentProviderOptions : BaseOptions
    {
        public string ConnectionString { get; set; }

        public string DataSource { get; set; }

        public string DatabaseName { get; set; }

        public string SchemaName { get; set; }

        public bool BufferedLoad { get; set; }

        public string TableName { get; set; }

        public string TableNamePrefix { get; set; }

        public string TableNameSuffix { get; set; }

        public bool UseTransactions { get; set; }

        public int DefaultTakeSize { get; set; }

        public int MaxTakeSize { get; set; }

        public bool EnableLogging { get; set; }

        //public SortColumn DefaultSortColumn { get; set; }

        public virtual string GetTableName<T>(string suffix = null)
        {
            var tableName = string.IsNullOrEmpty(this.TableName) ? typeof(T).Name.Pluralize() : this.TableName;
            if (!string.IsNullOrEmpty(this.TableNamePrefix))
            {
                tableName = this.TableNamePrefix + tableName;
            }

            if (!string.IsNullOrEmpty(this.TableNameSuffix))
            {
                tableName = tableName + this.TableNameSuffix;
            }

            if (!string.IsNullOrEmpty(suffix))
            {
                tableName = tableName + suffix;
            }

            return !string.IsNullOrEmpty(this.SchemaName)
                ? $"[{this.SchemaName}].[{tableName}]"
                : $"[{tableName}]";
        }
    }
}