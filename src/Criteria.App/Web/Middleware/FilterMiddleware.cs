namespace Naos.Core.App.Criteria.App.Web
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Naos.Core.Common;
    using Naos.Core.Filtering.App;

    /// <summary>
    /// Middleware which attempts to reads / creates a Correlation ID that can then be used in logs and
    /// passed to upstream requests.
    /// </summary>
    public class FilterMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<FilterMiddleware> logger;
        private readonly FilterMiddlewareOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterMiddleware"/> class.
        /// Creates a new instance of the FilterMiddleware.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="builder">The filter builder.</param>
        /// <param name="options">The configuration options.</param>
        public FilterMiddleware(
            RequestDelegate next,
            ILogger<FilterMiddleware> logger,
            IOptions<FilterMiddlewareOptions> options)
        {
            EnsureArg.IsNotNull(next, nameof(next));
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.next = next;
            this.logger = logger;
            this.options = options.Value ?? new FilterMiddlewareOptions();
        }

        /// <summary>
        /// Processes a request to create a <see cref="FilterContext"/> for the current request and disposes of it when the request is completing.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
        /// <param name="contextFactory">The <see cref="IFilterContextFactory"/> which can create a <see cref="FilterContext"/>.</param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context, IFilterContextFactory contextFactory)
        {
            var critarias = this.BuildCriterias(context);
            var orderBy = this.BuildOrderBy(context);

            contextFactory.Create(critarias, orderBy); // TODO: skip/take
            await this.next(context);

            contextFactory.Dispose();
        }

        private IEnumerable<Criteria> BuildCriterias(HttpContext context)
        {
            if (context.Request?.Query?.ContainsKey(this.options.CriteriaQueryStringKey) == false)
            {
                return Enumerable.Empty<Criteria>();
            }

            // correlationId=eq:2b34cc25-cd06-475c-8f9c-c42791f49b46,timestamp=qte:01-01-1980,level=eq:debug,OR,level=eq:information
            var query = context.Request.Query.FirstOrDefault(p => p.Key.Equals(this.options.CriteriaQueryStringKey, StringComparison.OrdinalIgnoreCase));
            var items = query.Value.ToString().Split(',');

            var result = new List<Criteria>();
            foreach (var item in items.Where(c => !c.IsNullOrEmpty()))
            {
                var name = item.SubstringTill("=");
                var value = item.SubstringFrom("=");
                var @operator = value.Contains(":") ? value.SubstringTill(":").Trim() : "eq";

                // TODO: AND / OR

                result.Add(new Criteria(
                    name.Trim(),
                    Enum.TryParse(@operator, true, out CriteriaOperator e) ? e : CriteriaOperator.Eq,
                    (value.Contains(":") ? value.SubstringFrom(":") : value).Trim().EmptyToNull()));
            }

            return result;
        }

        private IEnumerable<OrderBy> BuildOrderBy(HttpContext context)
        {
            if (context.Request?.Query?.ContainsKey(this.options.OrderByQueryStringKey) == false)
            {
                return Enumerable.Empty<OrderBy>();
            }

            // orderBy=desc:timestamp,level
            var query = context.Request.Query.FirstOrDefault(p => p.Key.Equals(this.options.OrderByQueryStringKey, StringComparison.OrdinalIgnoreCase));
            var items = query.Value.ToString().Split(',');

            var result = new List<OrderBy>();
            foreach (var item in items.Where(c => !c.IsNullOrEmpty()))
            {
                var name = item.Contains(":") ? item.SubstringFrom(":").Trim() : item;
                var direction = item.Contains(":") ? item.SubstringTill(":").Trim() : "ascending";

                result.Add(new OrderBy(
                    name.Trim(),
                    Enum.TryParse(direction, true, out OrderByDirection e) ? e : OrderByDirection.Ascending));
            }

            return result;
        }
    }
}
