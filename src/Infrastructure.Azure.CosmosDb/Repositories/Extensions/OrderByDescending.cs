namespace Naos.Core.Infrastructure.Azure.CosmosDb
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    public static partial class Extensions
    {
        public static IQueryable<TSource> OrderByDescending<TSource, TKey>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, TKey>> expression)
        {
            if (expression != null)
            {
                return source.OrderByDescending(expression);
            }

            return source;
        }
    }
}
