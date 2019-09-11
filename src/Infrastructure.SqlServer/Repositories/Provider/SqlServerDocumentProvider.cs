namespace Naos.Foundation.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using System.Threading.Tasks;
    using Dapper;
    using EnsureThat;
    using Microsoft.Extensions.Logging;

    public class SqlServerDocumentProvider<T> : IDocumentProvider<T>
    {
        private bool isInitialized;

        public SqlServerDocumentProvider(SqlServerDocumentProviderOptions<T> options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNullOrEmpty(options.ConnectionString, nameof(options.ConnectionString));
            EnsureArg.IsNotNull(options.SqlBuilder, nameof(options.SqlBuilder));

            this.Options = options;
            this.Options.Serializer ??= new JsonNetSerializer(TypedJsonSerializerSettings.Create());
            this.Logger = options.CreateLogger<SqlServerDocumentProvider<T>>();
        }

        public SqlServerDocumentProvider(Builder<SqlServerDocumentProviderOptionsBuilder<T>, SqlServerDocumentProviderOptions<T>> optionsBuilder)
            : this(optionsBuilder(new SqlServerDocumentProviderOptionsBuilder<T>()).Build())
        {
        }

        protected ILogger<SqlServerDocumentProvider<T>> Logger { get; }

        protected SqlServerDocumentProviderOptions<T> Options { get; }

        public async Task ResetAsync(bool indexOnly = false)
        {
            await this.Initialize().AnyContext();

            if (!indexOnly)
            {
                await this.DeleteTable(this.Options.DatabaseName, this.Options.GetTableName()).AnyContext();
                await this.Initialize(true).AnyContext();
                return;
            }

            await this.DeleteIndex(this.Options.DatabaseName, this.Options.GetTableName()).AnyContext();
            await this.Initialize(true).AnyContext();
        }

        public async Task<long> CountAsync(
            Expression<Func<T, bool>> expression,
            IEnumerable<string> tags = null)
        {
            return await this.CountAsync(new[] { expression }, tags).AnyContext();
        }

        public async Task<long> CountAsync(
        IEnumerable<Expression<Func<T, bool>>> expressions = null,
        IEnumerable<string> tags = null)
        {
            await this.Initialize().AnyContext();

            var sql = new StringBuilder($"{this.Options.SqlBuilder.BuildUseDatabase(this.Options.DatabaseName)} SELECT COUNT(*) FROM {this.Options.GetTableName()} WHERE [id]>0");
            foreach (var t in tags.Safe())
            {
                sql.Append(this.Options.SqlBuilder.BuildTagSelect(t));
            }

            foreach (var e in expressions.Safe())
            {
                sql.Append(this.Options.SqlBuilder.BuildCriteriaSelect(e, this.Options.IndexMaps));
            }

            using (var conn = await this.CreateConnectionAsync().AnyContext())
            {
                return conn.Query<int>(sql.ToString()).SingleOrDefault();
            }
        }

        public async Task<bool> ExistsAsync(object key, IEnumerable<string> tags = null)
        {
            EnsureArg.IsNotNull(key, nameof(key));

            await this.Initialize().AnyContext();

            var sql = new StringBuilder($"{this.Options.SqlBuilder.BuildUseDatabase(this.Options.DatabaseName)} SELECT [id] FROM {this.Options.GetTableName()} WHERE [key]='{key}'");
            foreach (var t in tags.Safe())
            {
                sql.Append(this.Options.SqlBuilder.BuildTagSelect(t));
            }

            using (var conn = await this.CreateConnectionAsync().AnyContext())
            {
                return conn.Query<int>(sql.ToString(), new { key }).Any();
            }
        }

        public async Task<IEnumerable<object>> LoadKeysAsync(
            Expression<Func<T, bool>> expression = null,
            IEnumerable<string> tags = null,
            int? skip = null,
            int? take = null,
            Expression<Func<T, object>> orderExpression = null,
            bool orderDescending = false)
        {
            return await this.LoadKeysAsync(new[] { expression }, tags, skip, take, orderExpression, orderDescending).AnyContext();
        }

        public async Task<IEnumerable<object>> LoadKeysAsync(
            IEnumerable<Expression<Func<T, bool>>> expressions = null,
            IEnumerable<string> tags = null,
            int? skip = null,
            int? take = null,
            Expression<Func<T, object>> orderExpression = null,
            bool orderDescending = false)
        {
            await this.Initialize().AnyContext();

            var sql = new StringBuilder($"{this.Options.SqlBuilder.BuildUseDatabase(this.Options.DatabaseName)} SELECT [key] FROM {this.Options.GetTableName()} WHERE [id]>0");
            foreach (var t in tags.Safe())
            {
                sql.Append(this.Options.SqlBuilder.BuildTagSelect(t));
            }

            foreach (var e in expressions.Safe())
            {
                sql.Append(this.Options.SqlBuilder.BuildCriteriaSelect(e, this.Options.IndexMaps));
            }

            sql.Append(this.Options.SqlBuilder.BuildOrderingSelect(orderExpression, orderDescending, this.Options.IndexMaps));
            sql.Append(this.Options.SqlBuilder.BuildPagingSelect(skip, take, this.Options.DefaultTakeSize, this.Options.MaxTakeSize));

            using (var conn = await this.CreateConnectionAsync().AnyContext())
            {
                return conn.Query<object>(sql.ToString());
            }
        }

        public async Task<IEnumerable<Stream>> LoadDataAsync(
            Expression<Func<T, bool>> expression,
            IEnumerable<string> tags = null,
            int? skip = null,
            int? take = null,
            Expression<Func<T, object>> orderExpression = null,
            bool orderDescending = false)
        {
            return await this.LoadDataAsync(null, new[] { expression }, tags, skip, take, orderExpression, orderDescending).AnyContext();
        }

        public async Task<IEnumerable<Stream>> LoadDataAsync(
            IEnumerable<Expression<Func<T, bool>>> expressions = null,
            IEnumerable<string> tags = null,
            int? skip = null,
            int? take = null,
            Expression<Func<T, object>> orderExpression = null,
            bool orderDescending = false)
        {
            return await this.LoadDataAsync(null, expressions, tags, skip, take, orderExpression, orderDescending).AnyContext();
        }

        public async Task<IEnumerable<Stream>> LoadDataAsync(
            object key,
            IEnumerable<Expression<Func<T, bool>>> expressions = null,
            IEnumerable<string> tags = null,
            int? skip = null,
            int? take = null,
            Expression<Func<T, object>> orderExpression = null,
            bool orderDescending = false)
        {
            await this.Initialize().AnyContext();

            var sql = new StringBuilder();
            if (key != null)
            {
                sql.Append($"{this.Options.SqlBuilder.BuildUseDatabase(this.Options.DatabaseName)} {this.Options.SqlBuilder.BuildDataSelectByKey(this.Options.GetTableName())}");
            }
            else
            {
                sql.Append($"{this.Options.SqlBuilder.BuildUseDatabase(this.Options.DatabaseName)} {this.Options.SqlBuilder.BuildDataSelectByTags(this.Options.GetTableName())}");
            }

            foreach (var t in tags.Safe())
            {
                sql.Append(this.Options.SqlBuilder.BuildTagSelect(t));
            }

            foreach (var e in expressions.Safe())
            {
                sql.Append(this.Options.SqlBuilder.BuildCriteriaSelect(e, this.Options.IndexMaps));
            }

            //sql.Append(this.Options.SqlBuilder.BuildFromTillDateTimeSelect(fromDateTime, tillDateTime));
            sql.Append(this.Options.SqlBuilder.BuildOrderingSelect(orderExpression, orderDescending, this.Options.IndexMaps));
            sql.Append(this.Options.SqlBuilder.BuildPagingSelect(skip, take, this.Options.DefaultTakeSize, this.Options.MaxTakeSize));

            using (var conn = await this.CreateConnectionAsync().AnyContext())
            {
                var results = conn.Query<byte[]>(sql.ToString(), new { key }/*, buffered: this.Options.BufferedLoad*/);
                if (results == null)
                {
                    return Enumerable.Empty<Stream>(); //TODO: yield break;
                }

                //foreach (var data in results.Where(data => data != null))
                //{
                //    yield return new MemoryStream(CompressionHelper.Decompress(data));
                //}

                return results.Safe().Select(d => new MemoryStream(CompressionHelper.Decompress(d)));
            }
        }

        public async Task<IEnumerable<T>> LoadValuesAsync(
            Expression<Func<T, bool>> expression,
            IEnumerable<string> tags = null,
            int? skip = null,
            int? take = null,
            Expression<Func<T, object>> orderExpression = null,
            bool orderDescending = false)
        {
            return await this.LoadValuesAsync(null, new[] { expression }.AsEnumerable(), tags, skip, take, orderExpression, orderDescending).AnyContext();
        }

        public async Task<IEnumerable<T>> LoadValuesAsync(
        IEnumerable<Expression<Func<T, bool>>> expressions = null,
        IEnumerable<string> tags = null,
        int? skip = null,
        int? take = null,
        Expression<Func<T, object>> orderExpression = null,
        bool orderDescending = false)
        {
            return await this.LoadValuesAsync(null, expressions.AsEnumerable(), tags, skip, take, orderExpression, orderDescending).AnyContext();
        }

        public async Task<IEnumerable<T>> LoadValuesAsync(
            object key,
            IEnumerable<Expression<Func<T, bool>>> expressions = null,
            IEnumerable<string> tags = null,
            int? skip = null,
            int? take = null,
            Expression<Func<T, object>> orderExpression = null,
            bool orderDescending = false)
        {
            await this.Initialize().AnyContext();

            var sql = new StringBuilder();
            if (key != null)
            {
                sql.Append($"{this.Options.SqlBuilder.BuildUseDatabase(this.Options.DatabaseName)} {this.Options.SqlBuilder.BuildValueSelectByKey(this.Options.GetTableName())}");
            }
            else
            {
                sql.Append($"{this.Options.SqlBuilder.BuildUseDatabase(this.Options.DatabaseName)} {this.Options.SqlBuilder.BuildValueSelectByTags(this.Options.GetTableName())}");
            }

            foreach (var t in tags.Safe())
            {
                sql.Append(this.Options.SqlBuilder.BuildTagSelect(t));
            }

            foreach (var e in expressions.Safe())
            {
                sql.Append(this.Options.SqlBuilder.BuildCriteriaSelect(e, this.Options.IndexMaps));
            }

            //sql.Append(SqlBuilder.BuildFromTillDateTimeSelect(fromDateTime, tillDateTime));
            sql.Append(this.Options.SqlBuilder.BuildOrderingSelect(orderExpression, orderDescending, this.Options.IndexMaps));
            sql.Append(this.Options.SqlBuilder.BuildPagingSelect(skip, take, this.Options.DefaultTakeSize, this.Options.MaxTakeSize));

            using (var conn = await this.CreateConnectionAsync().AnyContext())
            {
                var results = conn.Query<string>(sql.ToString(), new { key }/*, buffered: this.Options.BufferedLoad*/);
                if (results == null)
                {
                    return Enumerable.Empty<T>(); //TODO: yield break;
                }

                //foreach (var result in results)
                //{
                //    yield return this.Options.Serializer.Deserialize<T>(result);
                //}

                return results.Select(r => this.Options.Serializer.Deserialize<T>(r));
            }
        }

        public async Task<ProviderAction> Upsert(object key, Stream data, IEnumerable<string> tags = null, bool forceInsert = false, DateTime? timestamp = null)
        {
            await this.Initialize().AnyContext();

            return await this.UpsertInternalAsync(key, data: data, tags: tags, forceInsert: forceInsert, timestamp: timestamp).AnyContext();
        }

        public async Task<ProviderAction> UpsertAsync(object key, T document, IEnumerable<string> tags = null, bool forceInsert = false, DateTime? timestamp = null)
        {
            await this.Initialize().AnyContext();

            return await this.UpsertInternalAsync(key, document: document, tags: tags, forceInsert: forceInsert, timestamp: timestamp).AnyContext();
        }

        public async Task<ProviderAction> UpsertAsync(object key, T document, Stream data, IEnumerable<string> tags = null, bool forceInsert = false, DateTime? timestamp = null)
        {
            await this.Initialize().AnyContext();

            return await this.UpsertInternalAsync(key, document: document, data: data, tags: tags, forceInsert: forceInsert, timestamp: timestamp).AnyContext();
        }

        public async Task<ProviderAction> DeleteAsync(object key, IEnumerable<string> tags = null)
        {
            await this.Initialize().AnyContext();

            throw new NotImplementedException();
        }

        public async Task<ProviderAction> DeleteAsync(IEnumerable<string> tags)
        {
            await this.Initialize().AnyContext();

            throw new NotImplementedException();
        }

        protected virtual async Task<IDbConnection> CreateConnectionAsync(bool openConnection = true)
        {
            var connection = new SqlConnection(this.Options.ConnectionString);
            if (openConnection)
            {
                await connection.OpenAsync().AnyContext();
            }

            return connection;
        }

        protected async Task Initialize(bool force = false)
        {
            if (!this.isInitialized || force) // TODO: lock
            {
                await this.EnsureDatabase(this.Options.DatabaseName).AnyContext();
                await this.EnsureSchema(this.Options.SchemaName, this.Options.DatabaseName).AnyContext();
                await this.EnsureTable(this.Options.DatabaseName, this.Options.GetTableName()).AnyContext();
                await this.EnsureIndex(this.Options.DatabaseName, this.Options.GetTableName()).AnyContext();

                this.isInitialized = true;
            }
        }

        protected virtual async Task<bool> TableExists(string databaseName, string tableName)
        {
            using (var connection = await this.CreateConnectionAsync().AnyContext())
            {
                this.Logger.LogInformation($"{tableName} exists [{connection.Database}]");

                return connection.Query<string>($"{this.Options.SqlBuilder.BuildUseDatabase(databaseName)} {this.Options.SqlBuilder.TableNamesSelect()}")
                        .Any(t => t.Equals(tableName, StringComparison.OrdinalIgnoreCase));
            }
        }

        protected virtual async Task EnsureDatabase(string databaseName)
        {
            EnsureArg.IsNotNull(databaseName, nameof(databaseName));

            using (var connection = await this.CreateConnectionAsync(false).AnyContext())
            {
                this.Logger.LogInformation($"{databaseName} ensure database [{connection.Database}]");

                this.EnsureOpenConnection(connection);

                if (connection.Query<string>($@"
    SELECT *
    FROM sys.databases
    WHERE name='{databaseName}'")
                    .Any())
                {
                    return;
                }

                try
                {
                    connection.Execute($"CREATE DATABASE [{databaseName}]");
                }
                catch (SqlException ex)
                {
                    // swallow
                    this.Logger.LogError(ex, $"create database: {ex.Message}");
                }
            }
        }

        protected virtual async Task EnsureSchema(string schemaName, string databaseName)
        {
            EnsureArg.IsNotNull(databaseName, nameof(databaseName));

            if (string.IsNullOrEmpty(schemaName))
            {
                return;
            }

            using (var connection = await this.CreateConnectionAsync().AnyContext())
            {
                this.Logger.LogInformation($"{schemaName} ensure schema [{connection.Database}]");
                if (connection.Query<string>($@"
    {this.Options.SqlBuilder.BuildUseDatabase(databaseName)}
    SELECT QUOTENAME(SCHEMA_NAME) AS Name
    FROM INFORMATION_SCHEMA.SCHEMATA")
                    .Any(t => t.Equals($"[{schemaName}]", StringComparison.OrdinalIgnoreCase)))
                {
                    // already exists
                    return;
                }

                try
                {
                    connection.Execute($"CREATE SCHEMA [{schemaName}] AUTHORIZATION dbo");
                }
                catch (SqlException ex)
                {
                    // swallow
                    this.Logger.LogError(ex, $"create schema: {ex.Message}");
                }
            }
        }

        protected virtual async Task EnsureTable(string databaseName, string tableName)
        {
            if (await this.TableExists(databaseName, tableName).AnyContext())
            {
                return;
            }

            using (var connection = await this.CreateConnectionAsync().AnyContext())
            {
                this.Logger.LogInformation($"{tableName} ensure table [{connection.Database}]"); // http://stackoverflow.com/questions/11938044/what-are-the-best-practices-for-using-a-guid-as-a-primary-key-specifically-rega
                var sql = string.Format(@"
    {0}
    CREATE TABLE {1}(
    [uid] UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL PRIMARY KEY NONCLUSTERED,
    [id] INTEGER NOT NULL IDENTITY(1,1),
    [key] NVARCHAR(512) NOT NULL,
    [tags] NVARCHAR(1024) NOT NULL,
    [hash] NVARCHAR(128),
    [timestamp] DATETIME DEFAULT CURRENT_TIMESTAMP NOT NULL,
    [value] NTEXT,
    [data] VARBINARY(MAX));

    CREATE UNIQUE CLUSTERED INDEX [IX_id_{2}] ON {1} (id)
    CREATE INDEX [IX_key_{2}] ON {1} ([key] ASC);
    CREATE INDEX [IX_tags_{2}] ON {1} ([tags] ASC);
    CREATE INDEX [IX_hash_{2}] ON {1} ([hash] ASC);",
                    this.Options.SqlBuilder.BuildUseDatabase(databaseName),
                    tableName, new Random().Next(1000, 9999).ToString());
                connection.Execute(sql);
            }
        }

        protected virtual async Task EnsureIndex(string databaseName, string tableName)
        {
            if (this.Options.IndexMaps.IsNullOrEmpty())
            {
                return;
            }

            if (!await this.TableExists(databaseName, tableName).AnyContext())
            {
                await this.EnsureTable(databaseName, tableName).AnyContext();
            }

            using (var connection = await this.CreateConnectionAsync().AnyContext())
            {
                this.Logger.LogInformation($"{tableName} ensure index [{connection.Database}], index={this.Options.IndexMaps.Select(i => i.Name).ToString(", ")}");
                var sql = this.Options.IndexMaps.Select(i => string.Format(@"
    {0}
    IF NOT EXISTS(SELECT * FROM sys.columns
            WHERE Name = N'{2}{3}' AND Object_ID = Object_ID(N'{1}'))
    BEGIN
        ALTER TABLE {1} ADD [{2}{3}] {4}
        CREATE INDEX [IX_{2}{3}] ON {1} ([{2}{3}] ASC)
    END ", this.Options.SqlBuilder.BuildUseDatabase(databaseName), tableName, i.Name.ToLower(), this.Options.SqlBuilder.IndexColumnNameSuffix, this.Options.SqlBuilder.ToDbType(i.Type)));
                sql.ForEach(s => connection.Execute(s));
            }

            // sqlite check column exists: http://stackoverflow.com/questions/18920136/check-if-a-column-exists-in-sqlite
            // sqlite alter table https://www.sqlite.org/lang_altertable.html
        }

        protected async Task DeleteTable(string databaseName, string tableName)
        {
            if (!await this.TableExists(databaseName, tableName).AnyContext())
            {
                return;
            }

            using (var connection = await this.CreateConnectionAsync().AnyContext())
            {
                this.Logger.LogInformation($"{tableName} drop table [{connection.Database}]");

                var sql = string.Format(@"{0} DROP TABLE {1}", this.Options.SqlBuilder.BuildUseDatabase(databaseName), tableName);
                connection.Execute(sql);
            }
        }

        protected async Task DeleteIndex(string databaseName, string tableName)
        {
            if (!await this.TableExists(databaseName, tableName).AnyContext())
            {
                return;
            }

            using (var connection = await this.CreateConnectionAsync().AnyContext())
            {
                connection.Open();
                this.Logger.LogInformation($"{tableName} drop table [{connection.Database}]");

                var sql = this.Options.IndexMaps.Safe().Select(i =>
                    string.Format(@"
    {0}
    IF EXISTS(SELECT * FROM sys.columns
            WHERE Name = N'{2}{3}' AND Object_ID = Object_ID(N'{1}'))
    BEGIN
        DROP INDEX {1}.[IX_{2}{3}]
        ALTER TABLE {1} DROP COLUMN [{2}{3}]
    END ", this.Options.SqlBuilder.BuildUseDatabase(databaseName), tableName, i.Name.ToLower(), this.Options.SqlBuilder.IndexColumnNameSuffix));
                sql.ForEach(s => connection.Execute(s));
            }
        }

        protected virtual void EnsureOpenConnection(IDbConnection connection)
        {
            try
            {
                connection.Open();
            }
            catch (SqlException ex) // cannot login (catalog does not exist?), try without catalog in the conn string
            {
                var builder = new SqlConnectionStringBuilder(this.Options.ConnectionString);
                //if (builder.InitialCatalog.IsNullOrEmpty())
                //{
                //    throw;
                //}

                this.Logger.LogWarning($"fallback to db connectionstring with an empty initial catalog: {ex.Message}");

                builder.InitialCatalog = string.Empty;
                //this.Options.ConnectionString = builder.ConnectionString;
                //connection.ConnectionString = this.Options.ConnectionString;
                connection.ConnectionString = builder.ConnectionString;
                connection.Open();
            }
        }

        private async Task<ProviderAction> UpsertInternalAsync(object key, T document = default, Stream data = null, IEnumerable<string> tags = null, bool forceInsert = false, DateTime? timestamp = null)
        {
            EnsureArg.IsNotNull(key, nameof(key));

            // http://www.databasejournal.com/features/mssql/using-the-merge-statement-to-perform-an-upsert.html
            // http://stackoverflow.com/questions/2479488/syntax-for-single-row-merge-upsert-in-sql-server
            using (var connection = await this.CreateConnectionAsync().AnyContext())
            {
                ProviderAction result;
                var sql = new StringBuilder();
                if (!forceInsert && await this.ExistsAsync(key, tags).AnyContext())
                {
                    result = this.Update(key, document, data, tags, sql);
                }
                else
                {
                    result = this.Insert(key, document, data, tags, sql);
                }

                // PARAMS
                var parameters = new DynamicParameters();
                parameters.Add("key", key.ToString().SafeSubstring(0, 512));
                parameters.Add("tags", $"||{tags.ToString("||")}||".SafeSubstring(0, 1028));
                parameters.Add("hash", HashAlgorithm.ComputeHash(document).SafeSubstring(0, 128));
                parameters.Add("timestamp", timestamp ?? DateTime.UtcNow);
                parameters.Add("value", this.Options.Serializer.SerializeToString(document));
                parameters.Add("data", CompressionHelper.Compress(StreamHelper.ToBytes(data)), DbType.Binary);
                this.AddIndexParameters(document, parameters);

                connection.Execute(sql.ToString(), parameters);
                return result;
            }
        }

        private ProviderAction Update(object key, T document, Stream data, IEnumerable<string> tags, StringBuilder sql)
        {
            // UPDATE ===
            this.Logger.LogInformation($"{this.Options.GetTableName()} update: key={key},tags={tags?.ToString("||")}");

            var updateColumns = "[value]=@value";
            if (document != null && data != null)
            {
                updateColumns = $"{updateColumns},[data]=@data";
            }

            if (document == null && data != null)
            {
                updateColumns = "[data]=@data";
            }

#pragma warning disable SA1513 // Closing brace should be followed by blank line
            sql.Append(
                $@"
    {this.Options.SqlBuilder.BuildUseDatabase(this.Options.DatabaseName)}
    UPDATE {this.Options.GetTableName()}
    SET [tags]=@tags,[hash]=@hash,[timestamp]=@timestamp,{updateColumns}
        {
                    this.Options.IndexMaps.Safe()
                        .Select(
                            i =>
                                ",[" + i.Name.ToLower() + this.Options.SqlBuilder.IndexColumnNameSuffix + "]=@" +
                                i.Name.ToLower() + this.Options.SqlBuilder.IndexColumnNameSuffix)
                        .ToString(string.Empty)
        }
    WHERE [key]=@key
");
            foreach (var t in tags.Safe())
            {
                sql.Append(this.Options.SqlBuilder.BuildTagSelect(t));
            }

            return ProviderAction.Updated;
        }

        private ProviderAction Insert(object key, T document, Stream data, IEnumerable<string> tags, StringBuilder sql)
        {
            // INSERT ===
            this.Logger.LogInformation($"{this.Options.GetTableName()} insert: key={key},tags={tags?.ToString("||")}");

            var insertColumns = "[value]";
            if (document != null && data != null)
            {
                insertColumns = $"{insertColumns},[data]" /*+= ",[data]"*/;
            }

            if (document == null && data != null)
            {
                insertColumns = "[data]";
            }

            var insertValues = "@value";
            if (document != null && data != null)
            {
                insertValues = $"{insertValues},@data" /*",@data"*/;
            }

            if (document == null && data != null)
            {
                insertValues = "@data";
            }

            sql.Append(
                $@"
    {this.Options.SqlBuilder.BuildUseDatabase(this.Options.DatabaseName)}
    INSERT INTO {this.Options.GetTableName()}
        ([key],[tags],[hash],[timestamp],{insertColumns}{
                    this.Options.IndexMaps.Safe()
                        .Select(i => ",[" + i.Name.ToLower() + this.Options.SqlBuilder.IndexColumnNameSuffix + "]")
                        .ToString(string.Empty)})
        VALUES(@key,@tags,@hash,@timestamp,{insertValues}{
                    this.Options.IndexMaps.Safe()
                        .Select(i => ",@" + i.Name.ToLower() + this.Options.SqlBuilder.IndexColumnNameSuffix)
                        .ToString(string.Empty)})");

            return ProviderAction.Inserted;
        }

        private void AddIndexParameters(T document, DynamicParameters parameters)
        {
            if (document == null)
            {
                this.AddNullIndexParameters(parameters);
                return;
            }

            if (this.Options.IndexMaps.IsNullOrEmpty())
            {
                return;
            }

            if (parameters == null)
            {
                parameters = new DynamicParameters();
            }

            var indexColumnValues = this.Options.IndexMaps.ToDictionary(i => i.Name, i => i.Expression(document).ToString()); // =only string values for now > dbtype!

            foreach (var item in this.Options.IndexMaps)
            {
                parameters.Add(
                    $"{item.Name.ToLower()}{this.Options.SqlBuilder.IndexColumnNameSuffix}",
                    indexColumnValues.FirstOrDefault(
                        i => i.Key.Equals(item.Name, StringComparison.OrdinalIgnoreCase))
                        .ValueOrDefault(i => i.Value).SafeSubstring(0, 900)); // prevent index 900 chars overflow
            }
        }

        private void AddNullIndexParameters(DynamicParameters parameters)
        {
            if (this.Options.IndexMaps.IsNullOrEmpty())
            {
                return;
            }

            if (parameters == null)
            {
                parameters = new DynamicParameters();
            }

            foreach (var item in this.Options.IndexMaps)
            {
                parameters.Add($"{item.Name.ToLower()}{this.Options.SqlBuilder.IndexColumnNameSuffix}", null);
            }
        }

        // sql connection https://stackoverflow.com/questions/45401543/implement-idbconnection-in-net-core
    }
}
