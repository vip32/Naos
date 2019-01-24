namespace Naos.Core.Infrastructure.EntityFramework
{
    using System.Collections.Generic;
    using System.Linq;
    using Naos.Core.Common;
    using Naos.Core.Domain.Repositories;

    public static partial class Extensions
    {
        public static IQueryable<T> OrderIf<T>(
            this IQueryable<T> source, IFindOptions<T> options)
        {
            if(options == null)
            {
                return source;
            }

            IOrderedEnumerable<T> orderedResult = null;
            foreach (var order in (options?.Orders ?? new List<OrderOption<T>>()).Insert(options?.Order))
            {
                    orderedResult = orderedResult == null
                            ? order.Direction == OrderDirection.Ascending
                                ? source.OrderBy(order.Expression.Compile()) // replace wit CompileFast()? https://github.com/dadhi/FastExpressionCompiler
                                : source.OrderByDescending(order.Expression.Compile())
                            : order.Direction == OrderDirection.Ascending
                                ? orderedResult.ThenBy(order.Expression.Compile())
                                : orderedResult.ThenByDescending(order.Expression.Compile());
            }

            return orderedResult != null ? orderedResult.AsQueryable() : source;
        }
    }
}
