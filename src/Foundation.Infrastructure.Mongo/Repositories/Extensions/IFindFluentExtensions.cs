namespace Naos.Foundation.Infrastructure
{
    using System.Collections.Generic;
    using MongoDB.Driver;
    using Naos.Foundation.Domain;

    public static class IFindFluentExtensions
    {
        public static IFindFluent<TEntity, TProjection> Sort<TEntity, TProjection>(
            this IFindFluent<TEntity, TProjection> source,
            IFindOptions<TEntity> options)
            where TEntity : class, IEntity, IAggregateRoot
        {
            if (options?.Order == null && options?.Orders.IsNullOrEmpty() == true)
            {
                return source;
            }

            foreach (var order in (options?.Orders ?? new List<OrderOption<TEntity>>()).Insert(options?.Order))
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
