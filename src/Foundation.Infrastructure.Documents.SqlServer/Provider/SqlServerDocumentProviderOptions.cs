namespace Naos.Foundation.Infrastructure
{
    using System.Collections.Generic;
    using Humanizer;

    public class SqlServerDocumentProviderOptions<T> : BaseOptions
    {
        private string calculatedTableName;

        public ISerializer Serializer { get; set; }

        public string ConnectionString { get; set; }

        public ISqlBuilder SqlBuilder { get; set; }

        public IEnumerable<IIndexMap<T>> IndexMaps { get; set; }

        public string DataSource { get; set; }

        public string DatabaseName { get; set; }

        public string SchemaName { get; set; } = "dbo";

        public bool BufferedLoad { get; set; }

        public string TableName { get; set; }

        public string TableNamePrefix { get; set; }

        public string TableNameSuffix { get; set; }

        public bool UseTransactions { get; set; }

        public int DefaultTakeSize { get; set; } = 1000;

        public int MaxTakeSize { get; set; } = 5000;

        public bool IsLoggingEnabled { get; set; }

        public string DefaultSortColumn { get; set; } = "id";

        public virtual string GetTableName(string suffix = null)
        {
            if (!this.calculatedTableName.IsNullOrEmpty())
            {
                return this.calculatedTableName;
            }

            this.calculatedTableName = string.IsNullOrEmpty(this.TableName) ? typeof(T).Name.Pluralize() : this.TableName;
            if (!string.IsNullOrEmpty(this.TableNamePrefix))
            {
                this.calculatedTableName = this.TableNamePrefix + this.calculatedTableName;
            }

            if (!string.IsNullOrEmpty(this.TableNameSuffix))
            {
                this.calculatedTableName += this.TableNameSuffix;
            }

            if (!string.IsNullOrEmpty(suffix))
            {
                this.calculatedTableName += suffix;
            }

            this.SchemaName ??= "dbo";
            this.calculatedTableName = !string.IsNullOrEmpty(this.SchemaName)
                ? $"[{this.SchemaName}].[{this.calculatedTableName }]"
                : $"[{this.calculatedTableName }]";

            return this.calculatedTableName;
        }
    }
}