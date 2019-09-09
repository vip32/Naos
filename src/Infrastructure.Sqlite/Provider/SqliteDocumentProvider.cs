namespace Naos.Foundation.Infrastructure
{
    using System.Data;
    using System.Data.SqlClient;
    using System.Data.SQLite;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;

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

        protected override async Task<IDbConnection> CreateConnectionAsync()
        {
            var connection = new SQLiteConnection(this.Options.ConnectionString);
            await connection.OpenAsync().AnyContext();
            return connection;
        }

        // sqlite https://www.bricelam.net/2015/04/29/sqlite-on-corefx.html
    }
}
