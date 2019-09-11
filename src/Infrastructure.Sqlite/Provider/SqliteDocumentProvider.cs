namespace Naos.Foundation.Infrastructure
{
    using System.Data;
    using System.Data.SQLite;
    using System.Threading.Tasks;

    public class SqliteDocumentProvider<T> : SqlServerDocumentProvider<T>
    {
        public SqliteDocumentProvider(SqlServerDocumentProviderOptions<T> options)
            : base(options)
        {
        }

        public SqliteDocumentProvider(Builder<SqlServerDocumentProviderOptionsBuilder<T>, SqlServerDocumentProviderOptions<T>> optionsBuilder)
            : base(optionsBuilder)
        {
        }

        protected override async Task<IDbConnection> CreateConnectionAsync(bool openConnection = true)
        {
            var connection = new SQLiteConnection(this.Options.ConnectionString);
            if (openConnection)
            {
                await connection.OpenAsync().AnyContext();
            }

            return connection;
        }

        // sqlite https://www.bricelam.net/2015/04/29/sqlite-on-corefx.html
    }
}
