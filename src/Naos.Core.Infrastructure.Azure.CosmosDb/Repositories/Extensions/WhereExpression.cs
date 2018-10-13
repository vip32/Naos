namespace Naos.Core.Infrastructure.Azure.CosmosDb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public static partial class Extensions
    {
        public static IQueryable<TSource> WhereExpression<TSource>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, bool>> expression)
        {
            if (expression != null)
            {
                return source.Where(expression);
            }

            return source;
        }

        public static IQueryable<TSource> WhereExpressions<TSource>(
            this IQueryable<TSource> source,
            IEnumerable<Expression<Func<TSource, bool>>> expressions)
        {
            if (expressions?.Any() == true)
            {
                foreach (var expression in expressions)
                {
                    source = source.Where(expression);
                }
            }

            return source;
        }
    }
}
