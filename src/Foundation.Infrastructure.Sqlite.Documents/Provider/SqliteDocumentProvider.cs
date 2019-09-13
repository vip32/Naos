namespace Naos.Foundation.Infrastructure
{
    using System;
    using System.Data;
    using System.Data.SQLite;
    using System.Threading.Tasks;
    using Dapper;
    using Microsoft.Extensions.Logging;

    public class SqliteDocumentProvider<T> : SqlServerDocumentProvider<T>
    {
        public SqliteDocumentProvider(SqliteDocumentProviderOptions<T> options)
            : base(options)
        {
            //this.Options.SqlBuilder ??= new SqliteBuilder(options.LoggerFactory);
        }

        //public SqliteDocumentProvider(Builder<SqliteDocumentProviderOptionsBuilder<T>, SqliteDocumentProviderOptions<T>> optionsBuilder)
        //{
        //}

        protected override async Task<IDbConnection> CreateConnectionAsync(bool openConnection = true)
        {
            var connection = new SQLiteConnection(this.Options.ConnectionString);
            if (openConnection)
            {
                await connection.OpenAsync().AnyContext();
            }

            return connection;
        }

        protected override Task EnsureSchema(string schemaName, string databaseName)
        {
            return Task.CompletedTask; // not needed
        }

        protected override Task EnsureDatabase(string databaseName)
        {
            return Task.CompletedTask; // not needed
        }

        protected override async Task EnsureTable(string databaseName, string tableName)
        {
            if (await this.TableExists(databaseName, tableName).AnyContext())
            {
                return;
            }

            var sql = string.Format(@"
    {0}
    CREATE TABLE {1}(
    --[uid] UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL PRIMARY KEY NONCLUSTERED,
    --[id] INTEGER NOT NULL IDENTITY(1,1),
    [key] NVARCHAR(512) NOT NULL,
    [tags] NVARCHAR(1024) NOT NULL,
    [hash] NVARCHAR(128),
    [timestamp] DATETIME DEFAULT CURRENT_TIMESTAMP NOT NULL,
    [value] NTEXT,
    [data] BLOB);

    --CREATE UNIQUE CLUSTERED INDEX [IX_id_{2}] ON {1} (id)
    --CREATE INDEX [IX_key_{2}] ON {1} ([key] ASC);
    --CREATE INDEX [IX_tags_{2}] ON {1} ([tags] ASC);
    --CREATE INDEX [IX_hash_{2}] ON {1} ([hash] ASC);",
                this.Options.SqlBuilder.BuildUseDatabase(databaseName),
                tableName, new Random().Next(1000, 9999).ToString());

            using (var connection = await this.CreateConnectionAsync().AnyContext())
            {
                this.Logger.LogInformation($"sql ensure table {tableName} [{connection.Database}]"); // http://stackoverflow.com/questions/11938044/what-are-the-best-practices-for-using-a-guid-as-a-primary-key-specifically-rega
                if (this.Options.IsLoggingEnabled)
                {
                    this.Logger.LogDebug($"sql document query: {sql}");
                }

                connection.Execute(sql);
            }
        }

        // sqlite https://www.bricelam.net/2015/04/29/sqlite-on-corefx.html
    }
}
