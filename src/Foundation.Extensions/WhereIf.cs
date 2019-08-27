namespace Naos.Foundation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public static partial class Extensions
    {
        public static IQueryable<T> WhereIf<T>(
            this IQueryable<T> source,
            Expression<Func<T, bool>> predicate,
            bool? condition = true)
        {
            if (condition == true && predicate != null)
            {
                return source.Where(predicate);
            }

            return source;
        }

        public static IQueryable<T> WhereIf<T>(
            this IQueryable<T> source,
            IEnumerable<Expression<Func<T, bool>>> predicates,
            bool? condition = true)
        {
            if (condition == true && predicates?.Any() == true)
            {
                foreach (var predicate in predicates)
                {
                    source = source.Where(predicate);
                }
            }

            return source;
        }

        public static IEnumerable<T> WhereIf<T>(
            this IEnumerable<T> source,
            Func<T, bool> predicate,
            bool? condition = true)
        {
            if (condition == true && predicate != null)
            {
                return source.Where(predicate);
            }

            return source;
        }

        public static IEnumerable<T> WhereIf<T>(
            this IEnumerable<T> source,
            IEnumerable<Func<T, bool>> predicates,
            bool? condition = true)
        {
            if (condition == true && predicates?.Any() == true)
            {
                foreach (var predicate in predicates)
                {
                    source = source.Where(predicate);
                }
            }

            return source;
        }
    }
}