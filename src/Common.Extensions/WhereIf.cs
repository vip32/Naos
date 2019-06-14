namespace Naos.Foundation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public static partial class Extensions
    {
        public static IQueryable<TSource> WhereIf<TSource>(
            this IQueryable<TSource> source, bool condition,
            Expression<Func<TSource, bool>> predicate)
        {
            if(condition)
            {
                return source.Where(predicate);
            }
            else
            {
                return source;
            }
        }

        public static IEnumerable<TSource> WhereIf<TSource>(
            this IEnumerable<TSource> source, bool condition,
            Func<TSource, bool> predicate)
        {
            if(condition)
            {
                return source.Where(predicate);
            }
            else
            {
                return source;
            }
        }
    }
}