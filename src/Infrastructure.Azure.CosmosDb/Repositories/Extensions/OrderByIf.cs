namespace Naos.Core.Infrastructure.Azure.CosmosDb
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    public static partial class Extensions
    {
        public static IQueryable<T> OrderByIf<T, TKey>(
            this IQueryable<T> source,
            Expression<Func<T, TKey>> expression, bool descending = false)
        {
            if (expression != null)
            {
                if (descending)
                {
                    return source.OrderByDescending(expression);
                }

                return source.OrderBy(expression);
            }

            return source;
        }
    }
}
