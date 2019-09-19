namespace Naos.Foundation.Application
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Tracing.Domain;

    public class HttpClientTracerHandler : DelegatingHandler
    {
        private readonly ILogger logger;
        private readonly ITracer tracer;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientTracerHandler"/> class.
        /// Constructs the <see cref="HttpClientTracerHandler"/> with a custom <see cref="ILogger"/> and the default <see cref="HttpClientTracerHandler"/>.
        /// </summary>
        /// <param name="logger">User defined <see cref="ILogger"/>.</param>
        public HttpClientTracerHandler(
            ILogger logger,
            ITracer tracer = null)
        {
            this.logger = logger;
            this.tracer = tracer;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //var correlationId = request.GetCorrelationId();
            var requestId = request.GetRequestId();

            using (var scope = this.tracer?.BuildSpan(
                        $"{LogTraceNames.Http} {request.Method.ToString().ToLowerInvariant()} {request.RequestUri.AbsolutePath} ({requestId})",
                        LogKeys.OutboundRequest,
                        SpanKind.Client).Activate(this.logger)) // TODO: get service name as operationname (servicedescriptor?)
            {
                if (scope?.Span?.SpanId.IsNullOrEmpty() != true)
                {
                    request.Headers.Add("x-spanid", scope.Span.SpanId);
                }

                // TODO: add trace/correlationid + spanid to httprequest = propagate (header)
                return await base.SendAsync(request, cancellationToken).AnyContext();
            }
        }
    }
}