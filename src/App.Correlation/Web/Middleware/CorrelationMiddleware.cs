namespace Naos.Core.App.Correlation.Web
{
    using System;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Primitives;
    using Naos.Core.Common;

    /// <summary>
    /// Middleware which attempts to reads / creates a Correlation ID that can then be used in logs and
    /// passed to upstream requests.
    /// </summary>
    public class CorrelationMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<CorrelationMiddleware> logger;
        private readonly CorrelationMiddlewareOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelationMiddleware"/> class.
        /// Creates a new instance of the CorrelationIdMiddleware.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="options">The configuration options.</param>
        public CorrelationMiddleware(
            RequestDelegate next,
            ILogger<CorrelationMiddleware> logger,
            IOptions<CorrelationMiddlewareOptions> options)
        {
            EnsureArg.IsNotNull(next, nameof(next));
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.next = next;
            this.logger = logger;
            this.options = options.Value ?? new CorrelationMiddlewareOptions();
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

            if (this.options.UpdateTraceIdentifier)
            {
                this.logger.LogDebug($"api request ({requestId}) now has traceIdentifier {correlationId}, was {context.TraceIdentifier}"); // TODO: move to request logging middleware (operations)
                context.TraceIdentifier = correlationId;
            }

            contextFactory.Create(correlationId, this.options.CorrelationHeader, requestId, this.options.RequestHeader);

            if (this.options.IncludeInResponse)
            {
                context.Response.OnStarting(() =>
                {
                    // add the response headers
                    if (!context.Response.Headers.ContainsKey(this.options.CorrelationHeader))
                    {
                        context.Response.Headers.Add(this.options.CorrelationHeader, correlationId);
                    }
                    if (!context.Response.Headers.ContainsKey(this.options.RequestHeader))
                    {
                        context.Response.Headers.Add(this.options.RequestHeader, requestId);
                    }

                    return Task.CompletedTask;
                });
            }

            using (this.logger.BeginScope($"{{{this.options.CorrelationLogPropertyName}}}{{{this.options.RequestLogPropertyName}}}", correlationId, requestId))
            {
                await this.next(context);
            }

            contextFactory.Dispose();
        }

        private StringValues EnsureCorrelationId(HttpContext httpContext)
        {
            var isFound = httpContext.Request.Headers.TryGetValue(this.options.CorrelationHeader, out var id);
            if (!isFound || StringValues.IsNullOrEmpty(id))
            {
                if (this.options.UseGuidAsCorrelationId)
                {
                    id = Guid.NewGuid().ToString();
                }
                else if (this.options.UseHashAsCorrelationId)
                {
                    id = HashAlgorithm.ComputeHash(httpContext.TraceIdentifier);
                }
                else
                {
                    id = httpContext.TraceIdentifier;
                }
            }

            return id;
        }

        private StringValues EnsureRequestId(HttpContext httpContext)
        {
            var isFound = httpContext.Request.Headers.TryGetValue(this.options.RequestHeader, out var id);
            if (!isFound || StringValues.IsNullOrEmpty(id))
            {
                id = RandomGenerator.GenerateString(this.options.RequestIdLength, this.options.RequestIdAlphanumeric);
            }

            return id;
        }
    }
}
