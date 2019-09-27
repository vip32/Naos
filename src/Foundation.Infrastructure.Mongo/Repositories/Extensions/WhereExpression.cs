namespace Naos.Foundation.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// IQueryable extensions.
    /// </summary>
    public static partial class Extensions
    {
        public static IQueryable<TEntity> WhereExpression<TEntity>(
            this IQueryable<TEntity> source,
            Expression<Func<TEntity, bool>> expression)
        {
            if (expression != null)
            {
                return source.Where(expression.Expand());
            }

            return source;
        }

        public static IQueryable<TEntity> WhereExpressions<TEntity>(
            this IQueryable<TEntity> source,
            IEnumerable<Expression<Func<TEntity, bool>>> expressions)
        {
            if (expressions?.Any() == true)
            {
                foreach (var expression in expressions)
                {
                    source = source.Where(expression.Expand());
                }
            }

            return source;
        }

        public static IQueryable<TEntity> WhereExpression<TEntity>(
            this IQueryable<TEntity> source,
            Func<TEntity, bool> expression)
        {
            if (expression != null)
            {
                return source.Where(e => expression(e));
            }

            return source;
        }

        public static IQueryable<TEntity> WhereExpressions<TEntity>(
            this IQueryable<TEntity> source,
            IEnumerable<Func<TEntity, bool>> expressions)
        {
            if (expressions?.Any() == true)
            {
                foreach (var expression in expressions)
                {
                    return source.Where(e => expression(e));
                }
            }

            return source;
        }
    }
}
