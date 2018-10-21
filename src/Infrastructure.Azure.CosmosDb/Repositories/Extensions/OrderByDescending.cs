namespace Naos.Core.Infrastructure.Azure.CosmosDb
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    public static partial class Extensions
    {
        public static IQueryable<T> OrderByDescending<T, TKey>(
            this IQueryable<T> source,
            Expression<Func<T, TKey>> expression)
        {
            if (expression != null)
            {
                return source.OrderByDescending(expression);
            }

            return source;
        }
    }
}
