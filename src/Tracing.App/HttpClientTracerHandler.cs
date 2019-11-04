namespace Naos.Tracing.App
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Tracing.Domain;

    public class HttpClientTracerHandler : DelegatingHandler
    {
        private readonly ILogger logger;
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientTracerHandler"/> class.
        /// Constructs the <see cref="HttpClientTracerHandler"/> with a custom <see cref="ILogger"/> and the default <see cref="HttpClientTracerHandler"/>.
        /// </summary>
        /// <param name="logger">User defined <see cref="ILogger"/>.</param>
        public HttpClientTracerHandler(
            ILogger logger,
            IHttpContextAccessor httpContextAccessor)
        {
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if(this.httpContextAccessor?.HttpContext == null)
            {
                // TODO: get current scoped tracer in here! only happens in out of httpcontext request, for example inside a dequeued message handler.
                //       all fine within a httpcontext, also the with commands+handlers (only inside queued commands+handlers its an issue)
                this.logger.LogWarning($"{{LogKey:l}} no httpcontext available, cannot get current tracer (with parent span). this client http request will not be traced.", LogKeys.Tracing);
            }

            var tracer = this.httpContextAccessor?.HttpContext?.RequestServices.GetService<ITracer>();  // scoped workaround
            // WARN: proper scoped dependencies seem not possible in DelegatingHandlers https://github.com/aspnet/HttpClientFactory/issues/134
            //       that's why we need the httpContextAccessor to properly get a scoped ITracer (with current span set)

            if (tracer != null)
            {
                using (var scope = tracer.BuildSpan(
                            $"{LogTraceNames.Http} {request.Method.ToString().ToLowerInvariant()} {request.RequestUri.AbsoluteUri}",
                            LogKeys.OutboundRequest,
                            SpanKind.Client).Activate(this.logger)) // TODO: get service name as operationname (servicedescriptor?)
                {
                    if (scope?.Span != null)
                    {
                        //this.logger.LogDebug($"{{LogKey:l}} [{request.GetRequestId()}] http added tracing headers", LogKeys.OutboundRequest);

                        // propagate the span infos as headers
                        request.Headers.Add("x-traceid", scope.Span.TraceId);
                        request.Headers.Add("x-spanid", scope.Span.SpanId);
                        if (scope.Span.IsSampled.HasValue)
                        {
                            request.Headers.Add("x-tracesampled", scope.Span.IsSampled.Value.ToString());
                        }
                    }

                    return await base.SendAsync(request, cancellationToken).AnyContext();
                }
            }

            return await base.SendAsync(request, cancellationToken).AnyContext();
        }
    }
}