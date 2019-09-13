namespace Naos.Foundation.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public class SqliteDocumentProviderOptionsBuilder<T> :
        BaseOptionsBuilder<SqliteDocumentProviderOptions<T>, SqliteDocumentProviderOptionsBuilder<T>>
    {
        private readonly List<IIndexMap<T>> indexMaps = new List<IIndexMap<T>>();

        public SqliteDocumentProviderOptionsBuilder<T> Serializer(ISerializer serializer)
        {
            this.Target.Serializer = serializer;
            return this;
        }

        public SqliteDocumentProviderOptionsBuilder<T> SqlBuilder(ISqlBuilder sqlBuilder)
        {
            this.Target.SqlBuilder = sqlBuilder;
            return this;
        }

        public SqliteDocumentProviderOptionsBuilder<T> ConnectionString(string connectionString)
        {
            this.Target.ConnectionString = connectionString;
            this.Target.DatabaseName = connectionString.SliceFrom("Database=").SliceTill(";");
            this.Target.TableName = "documents";
            this.Target.SchemaName = null;
            this.Target.SqlBuilder = new SqliteBuilder(this.Target.LoggerFactory);
            return this;
        }

        public SqliteDocumentProviderOptionsBuilder<T> AddIndex(Expression<Func<T, object>> expression)
        {
            this.indexMaps.Add(new IndexMap<T>(expression));
            this.Target.IndexMaps = this.indexMaps.DistinctBy(i => i.Name);
            return this;
        }

        public SqliteDocumentProviderOptionsBuilder<T> EnableSqlLogging()
        {
            this.Target.IsLoggingEnabled = true;
            return this;
        }
    }
}