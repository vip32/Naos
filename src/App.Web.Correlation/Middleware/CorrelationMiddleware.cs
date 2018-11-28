namespace Naos.Core.App.Web.Correlation
{
    using System;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Primitives;

    /// <summary>
    /// Middleware which attempts to reads / creates a Correlation ID that can then be used in logs and
    /// passed to upstream requests.
    /// </summary>
    public class CorrelationMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<CorrelationMiddleware> logger;
        private readonly CorrelationOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelationMiddleware"/> class.
        /// Creates a new instance of the CorrelationIdMiddleware.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="options">The configuration options.</param>
        public CorrelationMiddleware(RequestDelegate next, ILogger<CorrelationMiddleware> logger, IOptions<CorrelationOptions> options)
        {
            EnsureArg.IsNotNull(next, nameof(next));
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.next = next;
            this.logger = logger;
            this.options = options.Value ?? new CorrelationOptions();
        }

        /// <summary>
        /// Processes a request to synchronise TraceIdentifier and Correlation ID headers. Also creates a
        /// <see cref="CorrelationContext"/> for the current request and disposes of it when the request is completing.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
        /// <param name="correlationContextFactory">The <see cref="ICorrelationContextFactory"/> which can create a <see cref="CorrelationContext"/>.</param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context, ICorrelationContextFactory correlationContextFactory)
        {
            var correlationId = this.SetCorrelationId(context);

            if (this.options.UpdateTraceIdentifier)
            {
                context.TraceIdentifier = correlationId;
            }

            correlationContextFactory.Create(correlationId, this.options.Header);

            if (this.options.IncludeInResponse)
            {
                // apply the correlation ID to the response header for client side tracking
                context.Response.OnStarting(() =>
                {
                    if (!context.Response.Headers.ContainsKey(this.options.Header))
                    {
                        context.Response.Headers.Add(this.options.Header, correlationId);
                    }

                    return Task.CompletedTask;
                });
            }

            using (this.logger.BeginScope("{CorrelationId}", correlationId))
            {
                await this.next(context);
            }

            correlationContextFactory.Dispose();
        }

        private static bool RequiresGenerationOfCorrelationId(bool isInHeader, StringValues headerId) =>
            !isInHeader || StringValues.IsNullOrEmpty(headerId);

        private StringValues SetCorrelationId(HttpContext context)
        {
            var found = context.Request.Headers.TryGetValue(this.options.Header, out var correlationId);

            if (RequiresGenerationOfCorrelationId(found, correlationId))
            {
                correlationId = this.GenerateCorrelationId(context.TraceIdentifier);
            }

            return correlationId;
        }

        private StringValues GenerateCorrelationId(string traceIdentifier) =>
            this.options.UseGuidForCorrelationId || string.IsNullOrEmpty(traceIdentifier) ? Guid.NewGuid().ToString() : traceIdentifier;
    }
}
