namespace Naos.Foundation.Infrastructure
{
    using System.Collections.Generic;
    using MongoDB.Driver;
    using Naos.Foundation.Domain;

    public static class IFindFluentExtensions
    {
        public static IFindFluent<TDocument, TProjection> Sort<TDocument, TProjection>(
            this IFindFluent<TDocument, TProjection> source,
            IFindOptions<TDocument> options)
        {
            if (options?.Order == null && options?.Orders.IsNullOrEmpty() == true)
            {
                return source;
            }

            foreach (var order in (options?.Orders ?? new List<OrderOption<TDocument>>()).Insert(options?.Order))
            {
                if (order.Direction == OrderDirection.Ascending)
                {
                    source.SortBy(order.Expression);
                }
                else
                {
                    source.SortByDescending(order.Expression);
                }
            }

            return source;
        }
    }
}
