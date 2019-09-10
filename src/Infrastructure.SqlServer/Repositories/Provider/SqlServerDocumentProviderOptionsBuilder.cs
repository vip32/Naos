namespace Naos.Foundation.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public class SqlServerDocumentProviderOptionsBuilder<T> :
        BaseOptionsBuilder<SqlServerDocumentProviderOptions<T>, SqlServerDocumentProviderOptionsBuilder<T>>
    {
        private readonly List<IIndexMap<T>> indexMaps = new List<IIndexMap<T>>();

        public SqlServerDocumentProviderOptionsBuilder<T> Serializer(ISerializer serializer)
        {
            this.Target.Serializer = serializer;
            return this;
        }

        public SqlServerDocumentProviderOptionsBuilder<T> ConnectionString(string connectionString)
        {
            this.Target.ConnectionString = connectionString;
            this.Target.DatabaseName = connectionString.SliceFrom("Database=").SliceTill(";");
            this.Target.SqlBuilder = new SqlBuilder();
            return this;
        }

        public SqlServerDocumentProviderOptionsBuilder<T> Schema(string schemaName = "dbo")
        {
            this.Target.SchemaName = schemaName;
            return this;
        }

        public SqlServerDocumentProviderOptionsBuilder<T> AddIndex(Expression<Func<T, object>> expression)
        {
            this.indexMaps.Add(new IndexMap<T>(expression));
            this.Target.IndexMaps = this.indexMaps.DistinctBy(i => i.Name);
            return this;
        }
    }
}