namespace Naos.Foundation.Infrastructure
{
    using System.Collections.Generic;

    public class SqlServerDocumentProviderOptionsBuilder<T> :
        BaseOptionsBuilder<SqlServerDocumentProviderOptions<T>, SqlServerDocumentProviderOptionsBuilder<T>>
    {
        public SqlServerDocumentProviderOptionsBuilder<T> Serializer(ISerializer serializer)
        {
            this.Target.Serializer = serializer;
            return this;
        }

        public SqlServerDocumentProviderOptionsBuilder<T> ConnectionString(string connectionString)
        {
            this.Target.ConnectionString = connectionString;
            this.Target.SqlBuilder = new SqlBuilder();
            return this;
        }

        public SqlServerDocumentProviderOptionsBuilder<T> IndexMap(IEnumerable<IIndexMap<T>> indexMap = null)
        {
            this.Target.IndexMap = indexMap;
            return this;
        }
    }
}