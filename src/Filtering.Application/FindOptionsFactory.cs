namespace Naos.RequestFiltering.Application
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    public static class FindOptionsFactory
    {
        public static IFindOptions<TEntity> Create<TEntity>(FilterContext filterContext)
            where TEntity : class, IEntity, IAggregateRoot
        {
            if (filterContext == null)
            {
                return new FindOptions<TEntity>();
            }

            return new FindOptions<TEntity>(skip: filterContext.Skip, take: filterContext.Take, orders: GetOrderOptions<TEntity>(filterContext));
        }

        private static IEnumerable<OrderOption<TEntity>> GetOrderOptions<TEntity>(FilterContext filterContext)
            where TEntity : class, IEntity, IAggregateRoot
        {
            var result = new List<OrderOption<TEntity>>();
            foreach (var order in filterContext.Orders.Safe().Where(o => !o.Name.IsNullOrEmpty()))
            {
                try
                {
                    result.Add(new OrderOption<TEntity>(
                        ExpressionHelper.GetExpression<TEntity>(order.Name),
                        order.Direction == OrderDirection.Asc ? Foundation.Domain.OrderDirection.Ascending : Foundation.Domain.OrderDirection.Descending));
                }
                catch (ArgumentException ex)
                {
                    throw new NaosClientFormatException(ex.Message, ex);
                }
            }

            return result;
        }
    }
}
