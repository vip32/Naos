namespace Naos.Foundation.Infrastructure
{
    public class SqlServerDocumentProviderOptionsBuilder :
        BaseOptionsBuilder<SqlServerDocumentProviderOptions, SqlServerDocumentProviderOptionsBuilder>
    {
        public SqlServerDocumentProviderOptionsBuilder ConnectionString(string connectionString)
        {
            this.Target.ConnectionString = connectionString;
            return this;
        }
    }
}