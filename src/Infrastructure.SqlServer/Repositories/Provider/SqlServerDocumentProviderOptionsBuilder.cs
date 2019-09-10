namespace Naos.Foundation.Infrastructure
{
    using System.Collections.Generic;

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
            this.Target.SqlBuilder = new SqlBuilder();
            return this;
        }

        public SqlServerDocumentProviderOptionsBuilder<T> AddIndex(IEnumerable<IIndexMap<T>> indexMap = null)
        {
            this.indexMaps.Add(indexMap);
            this.Target.IndexMaps = this.indexMaps.DistinctBy(i => i.Name);
            return this;
        }
    }
}