namespace Naos.RequestFiltering.Application
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    public static class FindOptionsFactory
    {
        public static IFindOptions<T> Create<T>(FilterContext filterContext)
        {
            if (filterContext == null)
            {
                return new FindOptions<T>();
            }

            return new FindOptions<T>(skip: filterContext.Skip, take: filterContext.Take, orders: GetOrderOptions<T>(filterContext));
        }

        private static IEnumerable<OrderOption<T>> GetOrderOptions<T>(FilterContext filterContext)
        {
            var result = new List<OrderOption<T>>();
            foreach (var order in filterContext.Orders.Safe().Where(o => !o.Name.IsNullOrEmpty()))
            {
                try
                {
                    result.Add(new OrderOption<T>(
                        ExpressionHelper.GetExpression<T>(order.Name),
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
