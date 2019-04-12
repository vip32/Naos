namespace Naos.Core.Commands.Correlation.App.Web
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Primitives;
    using Naos.Core.Common;
    using Naos.Core.Common.Web;
    using Naos.Core.RequestCorrelation.App;

    /// <summary>
    /// Middleware which attempts to reads / creates a Correlation ID that can then be used in logs and
    /// passed to upstream requests.
    /// </summary>
    public class RequestCorrelationMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<RequestCorrelationMiddleware> logger;
        private readonly RequestCorrelationMiddlewareOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestCorrelationMiddleware"/> class.
        /// Creates a new instance of the CorrelationIdMiddleware.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="options">The configuration options.</param>
        public RequestCorrelationMiddleware(
            RequestDelegate next,
            ILogger<RequestCorrelationMiddleware> logger,
            IOptions<RequestCorrelationMiddlewareOptions> options)
        {
            EnsureArg.IsNotNull(next, nameof(next));
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.next = next;
            this.logger = logger;
            this.options = options.Value ?? new RequestCorrelationMiddlewareOptions();
        }

        /// <summary>
        /// Processes a request to synchronise TraceIdentifier and Correlation ID headers. Also creates a
        /// <see cref="CorrelationContext"/> for the current request and disposes of it when the request is completing.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
        /// <param name="contextFactory">The <see cref="ICorrelationContextFactory"/> which can create a <see cref="CorrelationContext"/>.</param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context, ICorrelationContextFactory contextFactory)
        {
            var correlationId = this.EnsureCorrelationId(context);
            var requestId = this.EnsureRequestId(context);

            var loggerState = new Dictionary<string, object>
            {
                [LogEventPropertyKeys.CorrelationId] = correlationId,
                [LogEventPropertyKeys.RequestId] = requestId
            };

            using(this.logger.BeginScope(loggerState))
            {
                // needed by other request middlewares (requestresponselogging, filtering)
                context.SetCorrelationId(correlationId);
                context.SetRequestId(requestId);

                if(this.options.UpdateTraceIdentifier)
                {
                    this.logger.LogDebug($"{{LogKey:l}} [{requestId}] http now has traceIdentifier {correlationId}, was {context.TraceIdentifier}", LogKeys.InboundRequest); // TODO: move to request logging middleware (operations)
                    context.TraceIdentifier = correlationId;
                }

                contextFactory.Create(correlationId, this.options.CorrelationHeader, requestId, this.options.RequestHeader);

                if(this.options.IncludeInResponse)
                {
                    context.Response.OnStarting(() =>
                    {
                        // add the response headers
                        if(!context.Response.Headers.ContainsKey(this.options.CorrelationHeader))
                        {
                            context.Response.Headers.Add(this.options.CorrelationHeader, correlationId);
                        }
                        if(!context.Response.Headers.ContainsKey(this.options.RequestHeader))
                        {
                            context.Response.Headers.Add(this.options.RequestHeader, requestId);
                        }

                        return Task.CompletedTask;
                    });
                }

                await this.next(context);
            }

            contextFactory.Dispose();
        }

        private string EnsureCorrelationId(HttpContext httpContext)
        {
            var isFound = httpContext.Request.Headers.TryGetValue(this.options.CorrelationHeader, out var id);
            if(!isFound || StringValues.IsNullOrEmpty(id))
            {
                if(this.options.UseRandomCorrelationId)
                {
                    //return Guid.NewGuid().ToString(); //.Replace("-", string.Empty);
                    return IdGenerator.Instance.Next; //RandomGenerator.GenerateString(this.options.RandomCorrelationIdLength, true);
                }
                else if(this.options.UseHashAsCorrelationId)
                {
                    return HashAlgorithm.ComputeMd5Hash(httpContext.TraceIdentifier);
                }
                else
                {
                    return httpContext.TraceIdentifier;
                }
            }

            return id.ToString();
        }

        private string EnsureRequestId(HttpContext httpContext)
        {
            var isFound = httpContext.Request.Headers.TryGetValue(this.options.RequestHeader, out var id);
            if(!isFound || StringValues.IsNullOrEmpty(id))
            {
                return RandomGenerator.GenerateStringFast(this.options.RequestIdLength, this.options.RequestIdAlphanumeric);
            }

            return id.ToString();
        }
    }
}
