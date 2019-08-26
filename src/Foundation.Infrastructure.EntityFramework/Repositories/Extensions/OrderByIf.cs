﻿namespace Naos.Foundation.Infrastructure
{
    using System.Collections.Generic;
    using System.Linq;
    using Naos.Foundation.Domain;

    /// <summary>
    /// IQueryable extensions.
    /// </summary>
    public static partial class Extensions
    {
        public static IOrderedQueryable<T> OrderByIf<T>(
            this IQueryable<T> source,
            IFindOptions<T> options)
        {
            if(options?.Order == null && options?.Orders.IsNullOrEmpty() == true)
            {
                return source as IOrderedQueryable<T>; // TODO: this returns null, find a way to return an IOrderedQueryable event if no orders are provided. possible?
            }

            IOrderedQueryable<T> result = null;
            foreach(var order in (options?.Orders ?? new List<OrderOption<T>>()).Insert(options?.Order))
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
    }
}
