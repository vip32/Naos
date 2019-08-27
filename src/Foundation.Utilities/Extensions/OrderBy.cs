namespace Naos.Foundation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// Utility extensions.
    /// </summary>
    public static partial class UtilityExtensions
    {
        public static IOrderedEnumerable<TSource> OrderBy<TSource>(this IEnumerable<TSource> source, string name, bool ascending = true)
        {
            if (source == null || name.IsNullOrEmpty())
            {
                return null;
            }

            if (ascending)
            {
                return source.OrderBy(ExpressionHelper.GetFunc<TSource>(name));
            }
            else
            {
                return source.OrderByDescending(ExpressionHelper.GetFunc<TSource>(name));
            }
        }

        public static IOrderedQueryable<TSource> OrderBy<TSource>(this IQueryable<TSource> source, string name, bool ascending = true)
        {
            if (source == null || name.IsNullOrEmpty())
            {
                return null;
            }

            if (ascending)
            {
                return source.OrderBy(ExpressionHelper.GetExpression<TSource>(name));
            }
            else
            {
                return source.OrderByDescending(ExpressionHelper.GetExpression<TSource>(name));
            }
        }

        public static IQueryable<T> OrderByDynamic<T>(this IQueryable<T> source, string name, bool ascending = true)
        {
            var param = Expression.Parameter(typeof(T), "p");
            var prop = Expression.Property(param, name);
            var exp = Expression.Lambda(prop, param);
            var method = ascending ? "OrderBy" : "OrderByDescending";
            var types = new Type[] { source.ElementType, exp.Body.Type };
            var mce = Expression.Call(typeof(Queryable), method, types, source.Expression, exp);

            return source.Provider.CreateQuery<T>(mce);
        }
    }
}
