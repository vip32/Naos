namespace Naos.Core.Infrastructure.Azure.CosmosDb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public static partial class Extensions
    {
        public static IQueryable<TSource> WhereExpressionIf<TSource>(
            this IQueryable<TSource> source,
            bool condition,
            Expression<Func<TSource, bool>> expression)
        {
            if (condition && expression != null)
            {
                return source.Where(expression);
            }

            return source;
        }

        public static IQueryable<TSource> WhereExpressionsIf<TSource>(
            this IQueryable<TSource> source,
            bool condition,
            IEnumerable<Expression<Func<TSource, bool>>> expressions)
        {
            if (condition && expressions?.Any() == true)
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
