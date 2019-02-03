namespace Naos.Core.RequestFiltering.App.Web
{
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Naos.Core.Common;
    using Naos.Core.Common.Web;
    using Naos.Core.RequestFiltering.App;

    /// <summary>
    /// Middleware which attempts to reads / creates a Correlation ID that can then be used in logs and
    /// passed to upstream requests.
    /// </summary>
    public class RequestFilterMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<RequestFilterMiddleware> logger;
        private readonly RequestFilterMiddlewareOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestFilterMiddleware"/> class.
        /// Creates a new instance of the FilterMiddleware.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="options">The configuration options.</param>
        public RequestFilterMiddleware(
            RequestDelegate next,
            ILogger<RequestFilterMiddleware> logger,
            IOptions<RequestFilterMiddlewareOptions> options)
        {
            EnsureArg.IsNotNull(next, nameof(next));
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.next = next;
            this.logger = logger;
            this.options = options.Value ?? new RequestFilterMiddlewareOptions();
        }

        /// <summary>
        /// Processes a request to create a <see cref="FilterContext"/> for the current request and disposes of it when the request is completing.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
        /// <param name="contextFactory">The <see cref="IFilterContextFactory"/> which can create a <see cref="FilterContext"/>.</param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context, IFilterContextFactory contextFactory)
        {
            var filterContext = contextFactory.Create(context?.Request, this.options.CriteriaQueryStringKey, this.options.OrderByQueryStringKey, this.options.SkipQueryStringKey, this.options.TakeQueryStringKey);
            if (filterContext.Enabled)
            {
                this.logger.LogInformation($"{{LogKey:l}} [{context.GetRequestId()}] http filter={{@FilterContext}}", LogEventKeys.InboundRequest, filterContext);
            }

            await this.next(context);

            contextFactory.Dispose();
        }
    }
}
