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

        public Task ResetAsync(bool indexOnly = false)
        {
            throw new NotImplementedException();
        }

        public async Task<long> CountAsync(IEnumerable<Expression<Func<T, bool>>> expressions = null, IEnumerable<string> tags = null)
        {
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

        public async Task<IEnumerable<Stream>> LoadDataAsync(
            object key,
            IEnumerable<Expression<Func<T, bool>>> expressions = null,
            IEnumerable<string> tags = null,
            int? skip = null,
            int? take = null)
        {
            using (var conn = await this.CreateConnectionAsync().AnyContext())
            {
                var sql = new StringBuilder($"{this.Options.SqlBuilder.BuildUseDatabase(this.Options.DatabaseName)} {this.Options.SqlBuilder.BuildDataSelectByKey(this.Options.GetTableName())}");
                foreach (var t in tags.Safe())
                {
                    sql.Append(this.Options.SqlBuilder.BuildTagSelect(t));
                }

                foreach (var e in expressions.Safe())
                {
                    sql.Append(this.Options.SqlBuilder.BuildCriteriaSelect(e, this.Options.IndexMaps));
                }

                //sql.Append(this.Options.SqlBuilder.BuildFromTillDateTimeSelect(fromDateTime, tillDateTime));
                //sql.Append(this.Options.SqlBuilder.BuildSortingSelect(this.Options.DefaultSortColumn));
                sql.Append(this.Options.SqlBuilder.BuildPagingSelect(skip, take, this.Options.DefaultTakeSize, this.Options.MaxTakeSize));

                var results = conn.Query<byte[]>(sql.ToString(), new { key }, buffered: this.Options.BufferedLoad);
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

        public async Task<IEnumerable<object>> LoadKeysAsync(IEnumerable<Expression<Func<T, bool>>> expressions = null, IEnumerable<string> tags = null)
        {
            using (var conn = await this.CreateConnectionAsync().AnyContext())
            {
                var sql = new StringBuilder($"{this.Options.SqlBuilder.BuildUseDatabase(this.Options.DatabaseName)} SELECT [key] FROM {this.Options.GetTableName()} WHERE [id]>0");
                foreach (var t in tags.Safe())
                {
                    sql.Append(this.Options.SqlBuilder.BuildTagSelect(t));
                }

                foreach (var e in expressions.Safe())
                {
                    sql.Append(this.Options.SqlBuilder.BuildCriteriaSelect(e, this.Options.IndexMaps));
                }

                return conn.Query<object>(sql.ToString());
            }
        }

        public async Task<IEnumerable<T>> LoadValuesAsync(object key, IEnumerable<Expression<Func<T, bool>>> expressions = null, IEnumerable<string> tags = null, int? skip = 0, int? take = 0)
        {
            using (var conn = await this.CreateConnectionAsync().AnyContext())
            {
                var sql = new StringBuilder($"{this.Options.SqlBuilder.BuildUseDatabase(this.Options.DatabaseName)} {this.Options.SqlBuilder.BuildValueSelectByKey(this.Options.GetTableName())}");
                foreach (var t in tags.Safe())
                {
                    sql.Append(this.Options.SqlBuilder.BuildTagSelect(t));
                }

                foreach (var e in expressions.Safe())
                {
                    sql.Append(this.Options.SqlBuilder.BuildCriteriaSelect(e, this.Options.IndexMaps));
                }

                //sql.Append(SqlBuilder.BuildFromTillDateTimeSelect(fromDateTime, tillDateTime));
                //sql.Append(SqlBuilder.BuildSortingSelect(Options.DefaultSortColumn));
                //sql.Append(SqlBuilder.BuildPagingSelect(skip, take, Options.DefaultTakeSize, Options.MaxTakeSize));

                var results = conn.Query<string>(sql.ToString(), new { key }, buffered: this.Options.BufferedLoad);
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

        public async Task<IEnumerable<T>> LoadValuesAsync(
            IEnumerable<Expression<Func<T, bool>>> expressions = null,
            IEnumerable<string> tags = null,
            int? skip = null,
            int? take = null)
        {
            using (var conn = await this.CreateConnectionAsync().AnyContext())
            {
                var sql = new StringBuilder($"{this.Options.SqlBuilder.BuildUseDatabase(this.Options.DatabaseName)} {this.Options.SqlBuilder.BuildValueSelectByTags(this.Options.GetTableName())}");
                foreach (var t in tags.Safe())
                {
                    sql.Append(this.Options.SqlBuilder.BuildTagSelect(t));
                }

                foreach (var e in expressions.Safe())
                {
                    sql.Append(this.Options.SqlBuilder.BuildCriteriaSelect(e, this.Options.IndexMaps));
                }

                //sql.Append(this.Options.SqlBuilder.BuildFromTillDateTimeSelect(fromDateTime, tillDateTime));
                //sql.Append(this.Options.SqlBuilder.BuildSortingSelect(Options.DefaultSortColumn));
                sql.Append(this.Options.SqlBuilder.BuildPagingSelect(skip, take, this.Options.DefaultTakeSize, this.Options.MaxTakeSize));
                var documents = conn.Query<string>(sql.ToString(), buffered: this.Options.BufferedLoad);
                if (documents == null)
                {
                    return Enumerable.Empty<T>(); //TODO: yield break;
                }

                //foreach (var document in documents)
                //{
                //    yield return  this.Options.Serializer.Deserialize<T>(document);
                //}

                return documents.Select(d => this.Options.Serializer.Deserialize<T>(d));
            }
        }

        public Task<ProviderAction> Upsert(object key, Stream data, IEnumerable<string> tags = null, bool forceInsert = false, DateTime? timestamp = null)
        {
            throw new NotImplementedException();
        }

        public Task<ProviderAction> UpsertAsync(object key, T document, IEnumerable<string> tags = null, bool forceInsert = false, DateTime? timestamp = null)
        {
            throw new NotImplementedException();
        }

        public Task<ProviderAction> UpsertAsync(object key, T document, Stream data, IEnumerable<string> tags = null, bool forceInsert = false, DateTime? timestamp = null)
        {
            throw new NotImplementedException();
        }

        public Task<ProviderAction> DeleteAsync(object key, IEnumerable<string> tags = null)
        {
            throw new NotImplementedException();
        }

        public Task<ProviderAction> DeleteAsync(IEnumerable<string> tags)
        {
            throw new NotImplementedException();
        }

        protected virtual async Task<IDbConnection> CreateConnectionAsync()
        {
            var connection = new SqlConnection(this.Options.ConnectionString);
            await connection.OpenAsync().AnyContext();
            return connection;
        }

        // sql connection https://stackoverflow.com/questions/45401543/implement-idbconnection-in-net-core
    }

#pragma warning disable SA1201 // Elements should appear in the correct order
    public interface IDocumentProvider<T>
    {
        Task<long> CountAsync(IEnumerable<Expression<Func<T, bool>>> expressions = null, IEnumerable<string> tags = null/*, IEnumerable<Criteria> criterias = null*/);

        Task ResetAsync(bool indexOnly = false);

        Task<bool> ExistsAsync(object key, IEnumerable<string> tags = null);

        Task<ProviderAction> UpsertAsync(object key, T document, IEnumerable<string> tags = null, bool forceInsert = false,
            DateTime? timestamp = null);

        Task<ProviderAction> Upsert(object key, Stream data, IEnumerable<string> tags = null, bool forceInsert = false,
            DateTime? timestamp = null);

        Task<ProviderAction> UpsertAsync(object key, T document, Stream data, IEnumerable<string> tags = null,
            bool forceInsert = false, DateTime? timestamp = null);

        Task<IEnumerable<object>> LoadKeysAsync(IEnumerable<Expression<Func<T, bool>>> expressions = null, IEnumerable<string> tags = null);

        Task<IEnumerable<T>> LoadValuesAsync(object key, IEnumerable<Expression<Func<T, bool>>> expressions = null, IEnumerable<string> tags = null,
            //DateTime? fromDateTime = null, DateTime? tillDateTime = null,, IEnumerable<string> tags = null,
            //DateTime? fromDateTime = null, DateTime? tillDateTime = null,
            int? skip = 0, int? take = 0);

        Task<IEnumerable<Stream>> LoadDataAsync(object key, IEnumerable<Expression<Func<T, bool>>> expressions = null, IEnumerable<string> tags = null,
            //DateTime? fromDateTime = null, DateTime? tillDateTime = null,
            int? skip = null, int? take = null);

        Task<IEnumerable<T>> LoadValuesAsync(
            IEnumerable<Expression<Func<T, bool>>> expressions = null,
            IEnumerable<string> tags = null,
            //DateTime? fromDateTime = null, DateTime? tillDateTime = null,
            int? skip = null, int? take = null);

        Task<ProviderAction> DeleteAsync(object key, IEnumerable<string> tags = null/*, IEnumerable<Criteria> criterias = null*/); //IEnumerable<Expression<Func<T, bool>>> expressions = null

        Task<ProviderAction> DeleteAsync(IEnumerable<string> tags/*, IEnumerable<Criteria> criterias = null*/); // IEnumerable<Expression<Func<T, bool>>> expressions = null
    }
}
