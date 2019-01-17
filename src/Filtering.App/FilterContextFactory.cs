namespace Naos.Core.Filtering.App
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Http;
    using Naos.Core.Common;

    /// <inheritdoc />
    public class FilterContextFactory : IFilterContextFactory
    {
        private readonly IFilterContextAccessor accessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterContextFactory" /> class.
        /// </summary>
        public FilterContextFactory()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterContextFactory"/> class.
        /// </summary>
        /// <param name="accessor">The <see cref="IFilterContextAccessor"/> through which the <see cref="FilterContext"/> will be set.</param>
        public FilterContextFactory(IFilterContextAccessor accessor)
        {
            this.accessor = accessor;
        }

        /// <inheritdoc />
        public FilterContext Create(HttpRequest request, string criteriaQueryStringKey, string orderByQueryStringKey, string skipQueryStringKey, string takeQueryStringKey)
        {
            var result = new FilterContext
            {
                Criterias = this.BuildCriterias(request, criteriaQueryStringKey),
                OrderBy = this.BuildOrder(request, orderByQueryStringKey),
                Skip = request?.Query?.FirstOrDefault(p => p.Key.Equals(skipQueryStringKey, StringComparison.OrdinalIgnoreCase)).Value.FirstOrDefault().ToNullableInt(),
                Take = request?.Query?.FirstOrDefault(p => p.Key.Equals(takeQueryStringKey, StringComparison.OrdinalIgnoreCase)).Value.FirstOrDefault().ToNullableInt()
            };

            if (this.accessor != null)
            {
                this.accessor.Context = result;
            }

            return result;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (this.accessor != null)
            {
                this.accessor.Context = null;
            }
        }

        private IEnumerable<Criteria> BuildCriterias(HttpRequest request, string criteriaQueryStringKey)
        {
            if (request?.Query?.ContainsKey(criteriaQueryStringKey) == false)
            {
                return Enumerable.Empty<Criteria>();
            }

            // correlationId=eq:2b34cc25-cd06-475c-8f9c-c42791f49b46,timestamp=qte:01-01-1980,level=eq:debug,OR,level=eq:information
            var query = request.Query.FirstOrDefault(p => p.Key.Equals(criteriaQueryStringKey, StringComparison.OrdinalIgnoreCase));
            var items = query.Value.ToString().Split(',');

            var result = new List<Criteria>();
            foreach (var item in items.Where(c => !c.IsNullOrEmpty()))
            {
                if (item.EqualsAny(new[] { "and", "or" }))
                {
                    // TODO: AND / OR
                    continue;
                }

                var name = item.SubstringTill("=");
                var value = item.SubstringFrom("=");
                var @operator = value.Contains(":") ? value.SubstringTill(":").Trim() : "eq";

                result.Add(new Criteria(
                    name.Trim(),
                    Enum.TryParse(@operator, true, out CriteriaOperator e) ? e : CriteriaOperator.Eq,
                    (value.Contains(":") ? value.SubstringFrom(":") : value).Trim().EmptyToNull()));
            }

            return result;
        }

        private IEnumerable<Order> BuildOrder(HttpRequest request, string orderByQueryStringKey)
        {
            if (request?.Query?.ContainsKey(orderByQueryStringKey) == false)
            {
                return Enumerable.Empty<Order>();
            }

            // order=desc:timestamp,level
            var query = request.Query.FirstOrDefault(p => p.Key.Equals(orderByQueryStringKey, StringComparison.OrdinalIgnoreCase));
            var items = query.Value.ToString().Split(',');

            var result = new List<Order>();
            foreach (var item in items.Where(c => !c.IsNullOrEmpty()))
            {
                var name = item.Contains(":") ? item.SubstringFrom(":").Trim() : item;
                var direction = item.Contains(":") ? item.SubstringTill(":").Trim() : "ascending";

                result.Add(new Order(
                    name.Trim(),
                    Enum.TryParse(direction, true, out OrderDirection e) ? e : OrderDirection.Asc));
            }

            return result;
        }
    }
}