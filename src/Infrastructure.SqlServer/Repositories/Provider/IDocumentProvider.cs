namespace Naos.Foundation.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

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

        Task<IEnumerable<object>> LoadKeysAsync(
            IEnumerable<Expression<Func<T, bool>>> expressions = null,
            IEnumerable<string> tags = null,
            int? skip = null,
            int? take = null,
            Expression<Func<T, object>> orderExpression = null,
            bool orderDescending = false);

        Task<IEnumerable<T>> LoadValuesAsync(
            object key,
            IEnumerable<Expression<Func<T, bool>>> expressions = null,
            IEnumerable<string> tags = null,
            //DateTime? fromDateTime = null, DateTime? tillDateTime = null,, IEnumerable<string> tags = null,
            //DateTime? fromDateTime = null, DateTime? tillDateTime = null,
            int? skip = null,
            int? take = null,
            Expression<Func<T, object>> orderExpression = null,
            bool orderDescending = false);

        Task<IEnumerable<Stream>> LoadDataAsync(
            object key,
            IEnumerable<Expression<Func<T, bool>>> expressions = null,
            IEnumerable<string> tags = null,
            //DateTime? fromDateTime = null, DateTime? tillDateTime = null,
            int? skip = null,
            int? take = null,
            Expression<Func<T, object>> orderExpression = null,
            bool orderDescending = false);

        Task<IEnumerable<T>> LoadValuesAsync(
            Expression<Func<T, bool>> expression,
            IEnumerable<string> tags = null,
            //DateTime? fromDateTime = null, DateTime? tillDateTime = null,
            int? skip = null,
            int? take = null,
            Expression<Func<T, object>> orderExpression = null,
            bool orderDescending = false);

        Task<IEnumerable<T>> LoadValuesAsync(
            IEnumerable<Expression<Func<T, bool>>> expressions = null,
            IEnumerable<string> tags = null,
            //DateTime? fromDateTime = null, DateTime? tillDateTime = null,
            int? skip = null,
            int? take = null,
            Expression<Func<T, object>> orderExpression = null,
            bool orderDescending = false);

        Task<ProviderAction> DeleteAsync(object key, IEnumerable<string> tags = null/*, IEnumerable<Criteria> criterias = null*/); //IEnumerable<Expression<Func<T, bool>>> expressions = null

        Task<ProviderAction> DeleteAsync(IEnumerable<string> tags/*, IEnumerable<Criteria> criterias = null*/); // IEnumerable<Expression<Func<T, bool>>> expressions = null
    }
}
