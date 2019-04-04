namespace Naos.Core.RequestFiltering.App
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Naos.Core.Common;
    using Naos.Core.Domain.Repositories;

    public static class FindOptionsFactory
    {
        public static IFindOptions<T> Create<T>(FilterContext filterContext)
        {
            if(filterContext == null)
            {
                return new FindOptions<T>();
            }

            return new FindOptions<T>(skip: filterContext.Skip, take: filterContext.Take, orders: GetOrderOptions<T>(filterContext));
        }

        private static IEnumerable<OrderOption<T>> GetOrderOptions<T>(FilterContext filterContext)
        {
            var result = new List<OrderOption<T>>();
            foreach(var order in filterContext.Orders.Safe().Where(o => !o.Name.IsNullOrEmpty()))
            {
                try
                {
                    result.Add(new OrderOption<T>(
                        ExpressionHelper.GetExpression<T>(order.Name),
                        order.Direction == OrderDirection.Asc ? Domain.Repositories.OrderDirection.Ascending : Domain.Repositories.OrderDirection.Descending));
                }
                catch(ArgumentException ex)
                {
                    throw new NaosClientFormatException(ex.Message, ex);
                }
            }

            return result;
        }
    }
}
