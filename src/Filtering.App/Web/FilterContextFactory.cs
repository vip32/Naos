namespace Naos.Core.RequestFiltering.App.Web
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Application;

    /// <inheritdoc />
    public class FilterContextFactory : IFilterContextFactory
    {
        private readonly ILogger<FilterContextFactory> logger;
        private readonly IFilterContextAccessor accessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterContextFactory" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public FilterContextFactory(ILogger<FilterContextFactory> logger)
            : this(logger, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterContextFactory"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="accessor">The <see cref="IFilterContextAccessor"/> through which the <see cref="FilterContext"/> will be set.</param>
        public FilterContextFactory(ILogger<FilterContextFactory> logger, IFilterContextAccessor accessor)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
            this.accessor = accessor;
        }

        /// <inheritdoc />
        public FilterContext Create(
            HttpRequest request,
            string criteriaQueryStringKey = QueryStringKeys.Criteria,
            string orderQueryStringKey = QueryStringKeys.Order,
            string skipQueryStringKey = QueryStringKeys.Skip,
            string takeQueryStringKey = QueryStringKeys.Take)
        {
            var result = new FilterContext
            {
                Criterias = this.BuildCriterias(request, criteriaQueryStringKey),
                Orders = this.BuildOrders(request, orderQueryStringKey),
                Skip = request?.Query?.FirstOrDefault(p => p.Key.Equals(skipQueryStringKey, StringComparison.OrdinalIgnoreCase)).Value.FirstOrDefault().ToNullableInt(),
                Take = request?.Query?.FirstOrDefault(p => p.Key.Equals(takeQueryStringKey, StringComparison.OrdinalIgnoreCase)).Value.FirstOrDefault().ToNullableInt()
            };

            if(this.accessor != null)
            {
                this.accessor.Context = result;
            }

            return result;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if(this.accessor != null)
            {
                this.accessor.Context = null;
            }
        }

        private IEnumerable<Criteria> BuildCriterias(HttpRequest request, string queryStringKey)
        {
            if(request?.Query?.ContainsKey(queryStringKey) == false)
            {
                return Enumerable.Empty<Criteria>();
            }

            // correlationId=eq:2b34cc25-cd06-475c-8f9c-c42791f49b46,timestamp=qte:01-01-1980,level=eq:debug,OR,level=eq:information
            var query = request.Query.FirstOrDefault(p => p.Key.Equals(queryStringKey, StringComparison.OrdinalIgnoreCase));
            var items = query.Value.ToString().Split(',');

            var result = new List<Criteria>();
            foreach(var item in items.Where(c => !c.IsNullOrEmpty()))
            {
                if(item.EqualsAny(new[] { "and", "or" }))
                {
                    // TODO: AND / OR
                    continue;
                }

                var name = item.SliceTill("=");
                var value = item.SliceFrom("=");
                var @operator = value.Contains(":") ? value.SliceTill(":").Trim() : "eq";

                result.Add(
                    new Criteria(
                        name.Trim(),
                        CriteriaExtensions.FromAbbreviation(@operator),
                        (value.Contains(":") ? value.SliceFrom(":") : value).Trim().EmptyToNull()));
                // TODO: properly determine numeric oder not and pass to criteria
            }

            if(result.Count > 0)
            {
                this.logger.LogDebug($"{{LogKey:l}} [{request.HttpContext.GetRequestId()}] http filter criterias={result.Select(c => c.ToString()).ToString("|")}", LogKeys.InboundRequest);
            }

            return result;
        }

        private IEnumerable<Order> BuildOrders(HttpRequest request, string queryStringKey)
        {
            if(request?.Query?.ContainsKey(queryStringKey) == false)
            {
                return Enumerable.Empty<Order>();
            }

            // order=desc:timestamp,level
            var query = request.Query.FirstOrDefault(p => p.Key.Equals(queryStringKey, StringComparison.OrdinalIgnoreCase));
            var items = query.Value.ToString().Split(',');

            var result = new List<Order>();
            foreach(var item in items.Where(c => !c.IsNullOrEmpty()))
            {
                var name = item.Contains(":") ? item.SliceFrom(":").Trim() : item;
                var direction = item.Contains(":") ? item.SliceTill(":").Trim() : "ascending";

                result.Add(
                    new Order(
                        name.Trim(),
                        Enum.TryParse(direction, true, out OrderDirection e) ? e : OrderDirection.Asc));
            }

            if(result.Count > 0)
            {
                this.logger.LogDebug($"{{LogKey:l}} [{request.HttpContext.GetRequestId()}] http filter orders={result.Select(o => o.ToString()).ToString("|")}", LogKeys.InboundRequest);
            }

            return result;
        }

        //private bool IsNumeric(string value) => value.Safe().All(char.IsDigit);
    }
}