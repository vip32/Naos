namespace Naos.Core.Filtering.App
{
    using System.Collections.Generic;
    using System.Linq;
    using Naos.Core.Common;
    using Naos.Core.Domain.Repositories;
    using Naos.Core.Domain.Specifications;

    /// <summary>
    /// Provides access to the filter information.
    /// </summary>
    public class FilterContext
    {
        public IEnumerable<Criteria> Criterias { get; set; } = Enumerable.Empty<Criteria>();

        public IEnumerable<Order> Orders { get; set; } = Enumerable.Empty<Order>();

        public int? Skip { get; set; }

        public int? Take { get; set; }

        public bool Enabled
            => !this.Criterias.IsNullOrEmpty() || !this.Orders.IsNullOrEmpty() || this.Skip.HasValue || this.Take.HasValue;

        //// <summary>
        //// Converts the <see cref="IEnumerable<Criteria>"/> to a specification
        //// </summary>
        //// <typeparam name="T">The type of the entity.</typeparam>
        //// <returns></returns>
        public IEnumerable<Specification<T>> GetSpecifications<T>()
        {
            var result = new List<Specification<T>>();
            foreach (var criteria in this.Criterias.Safe())
            {
                result.Add(new Specification<T>(criteria.ToExpression<T>()));
            }

            return result;
        }

        //public Specification<T> GetCritertiasSpecification<T>()
        //{
        //    if (!this.Criterias.IsNullOrEmpty())
        //    {
        //        var specification = new Specification<T>(this.Criterias.First().ToExpression<T>());
        //        foreach (var criteria in this.Criterias.Where(c => c != this.Criterias.First()))
        //        {
        //            specification.And(new Specification<T>(criteria.ToExpression<T>())); // for now only AND is supported, could be read from criteria
        //        }

        //        return specification;
        //    }

        //    return null;
        //}

        public IFindOptions<T> GetFindOptions<T>() => new FindOptions<T>(skip: this.Skip, take: this.Take, orders: this.GetOrderOptions<T>());

        private IEnumerable<OrderOption<T>> GetOrderOptions<T>()
        {
            var result = new List<OrderOption<T>>();
            foreach (var order in this.Orders.Safe().Where(o => !o.Name.IsNullOrEmpty()))
            {
                result.Add(new OrderOption<T>(
                    ExpressionHelper.GetExpression<T>(order.Name),
                    order.Direction == OrderDirection.Asc ? Domain.Repositories.OrderDirection.Ascending : Domain.Repositories.OrderDirection.Descending));
            }

            return result;
        }
    }
}
