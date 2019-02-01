namespace Naos.Core.Filtering.App
{
    using System.Collections.Generic;
    using System.Linq;
    using Jokenizer.Net;
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

        public IFindOptions<T> GetFindOptions<T>()
        {
            return new FindOptions<T>(orders: this.GetOrderOptions<T>());
        }

        public IEnumerable<OrderOption<T>> GetOrderOptions<T>()
        {
            var result = new List<OrderOption<T>>();
            foreach (var order in this.Orders)
            {
                var expr = Evaluator.ToLambda<T, bool>(Tokenizer.Parse($"(t) => t.{order.Name} == \"blah\""));
                var expr1 = Evaluator.ToLambda<T, bool>(Tokenizer.Parse($"(t) => t.{order.Name} == 123"));
                var expr3 = Evaluator.ToLambda<T, string>(Tokenizer.Parse($"(t) => t.{order.Name}"));
                //var expr2 = Evaluator.ToLambda<T, object>(Tokenizer.Parse($"(t) => t.{order.Name}"));

                result.Add(new OrderOption<T>(
                    Evaluator.ToLambda<T, object>(Tokenizer.Parse($"(t) => t.{order.Name}")),
                    order.Direction == OrderDirection.Asc ? Domain.Repositories.OrderDirection.Ascending : Domain.Repositories.OrderDirection.Descending));
            }

            return result;
        }
    }
}
