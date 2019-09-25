namespace Naos.Foundation.Infrastructure
{
    using System.Collections.Generic;
    using System.Linq;
    using Naos.Foundation.Domain;

    /// <summary>
    /// IQueryable extensions.
    /// </summary>
    public static partial class Extensions
    {
        public static IOrderedQueryable<TEntity> OrderByIf<TEntity>(
            this IQueryable<TEntity> source,
            IFindOptions<TEntity> options)
        {
            if (options?.Order == null && options?.Orders.IsNullOrEmpty() == true)
            {
                return source as IOrderedQueryable<TEntity>; // TODO: this returns null, find a way to return an IOrderedQueryable event if no orders are provided. possible?
            }

            IOrderedQueryable<TEntity> result = null;
            foreach (var order in (options?.Orders ?? new List<OrderOption<TEntity>>()).Insert(options?.Order))
            {
                result = result == null
                        ? order.Direction == OrderDirection.Ascending
                            ? Queryable.OrderBy(source, order.Expression) // replace wit CompileFast()? https://github.com/dadhi/FastExpressionCompiler
                            : Queryable.OrderByDescending(source, order.Expression)
                        : order.Direction == OrderDirection.Ascending
                            ? result.ThenBy(order.Expression)
                            : result.ThenByDescending(order.Expression);
            }

            return result;
        }
    }
}
