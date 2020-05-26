namespace Naos.Foundation.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public interface ICosmosSqlProvider<T>
    {
        Task<T> GetByIdAsync(string id, object partitionKey = null);

        Task<T> UpsertAsync(T entity, object partitionKeyValue = null);

        Task<IEnumerable<T>> WhereAsync(
            Expression<Func<T, bool>> expression,
            int? skip = null,
            int? take = null,
            Expression<Func<T, object>> orderExpression = null,
            bool orderDescending = false,
            object partitionKeyValue = null);

        Task<IEnumerable<T>> WhereAsync(
            IEnumerable<Expression<Func<T, bool>>> expressions = null,
            int? skip = null,
            int? take = null,
            Expression<Func<T, object>> orderExpression = null,
            bool orderDescending = false,
            object partitionKeyValue = null);

        Task<IEnumerable<T>> WhereAsync( // OBSOLETE
            Expression<Func<T, bool>> expression,
            Expression<Func<T, T>> selector,
            int? skip = null,
            int? take = null,
            Expression<Func<T, object>> orderExpression = null,
            bool orderDescending = false,
            object partitionKeyValue = null);

        Task<int> CountAsync(
            IEnumerable<Expression<Func<T, bool>>> expressions = null,
            object partitionKeyValue = null);

        Task<bool> DeleteByIdAsync(string id, object partitionKeyValue = null);
    }
}