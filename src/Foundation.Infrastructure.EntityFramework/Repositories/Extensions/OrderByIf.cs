namespace Naos.Foundation.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Naos.Foundation.Domain;

    /// <summary>
    /// IQueryable extensions.
    /// </summary>
    public static partial class Extensions
    {
        public static IOrderedQueryable<TEntity> OrderByIf<TEntity>(
            this IQueryable<TEntity> source,
            IFindOptions<TEntity> options)
            where TEntity : class, IEntity, IAggregateRoot
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

        public static IOrderedQueryable<TDestination> OrderByIf<TEntity, TDestination>(
            this IQueryable<TDestination> source,
            IFindOptions<TEntity> options,
            IEntityMapper mapper)
            where TEntity : class, IEntity, IAggregateRoot
        {
            if (options?.Order == null && options?.Orders.IsNullOrEmpty() == true)
            {
                return source as IOrderedQueryable<TDestination>; // TODO: this returns null, find a way to return an IOrderedQueryable event if no orders are provided. possible?
            }

            IOrderedQueryable<TDestination> result = null;
            foreach (var order in (options?.Orders ?? new List<OrderOption<TEntity>>()).Insert(options?.Order))
            {
                result = result == null
                        ? order.Direction == OrderDirection.Ascending
                            ? Queryable.OrderBy(source, mapper.MapExpression<Expression<Func<TDestination, object>>>(order.Expression)) // replace wit CompileFast()? https://github.com/dadhi/FastExpressionCompiler
                            : Queryable.OrderByDescending(source, mapper.MapExpression<Expression<Func<TDestination, object>>>(order.Expression))
                        : order.Direction == OrderDirection.Ascending
                            ? result.ThenBy(mapper.MapExpression<Expression<Func<TDestination, object>>>(order.Expression))
                            : result.ThenByDescending(mapper.MapExpression<Expression<Func<TDestination, object>>>(order.Expression));
            }

            return result;
        }
    }
}
