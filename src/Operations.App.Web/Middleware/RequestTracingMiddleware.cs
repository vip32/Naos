namespace Naos.Core.Operations.App.Web
{
    using System;
    using System.Threading.Tasks;
    using Humanizer;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Extensions;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Naos.Core.Tracing.Domain;
    using Naos.Foundation;
    using Naos.Foundation.Application;

    public class RequestTracingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<RequestTracingMiddleware> logger;
        private readonly RequestTracingMiddlewareOptions options;

        public RequestTracingMiddleware(
            RequestDelegate next,
            ILogger<RequestTracingMiddleware> logger,
            IOptions<RequestTracingMiddlewareOptions> options)
        {
            this.next = next;
            this.logger = logger;
            this.options = options.Value ?? new RequestTracingMiddlewareOptions();
        }

        public async Task Invoke(HttpContext context, ITracer tracer)
        {
            if (!this.options.Enabled
                || tracer == null
                || context.Request.Path.Value.EqualsPatternAny(this.options.PathBlackListPatterns))
            {
                await this.next.Invoke(context).AnyContext();
            }
            else
            {
                var uri = context.Request.Uri();
                object action = null;
                object controller = null;
                context.GetRouteData()?.Values.TryGetValue("Action", out action);
                context.GetRouteData()?.Values.TryGetValue("Controller", out controller);

                using (var scope = tracer.BuildSpan(
                        $"{LogTraceNames.Http} {action ?? context.Request.Method.ToLowerInvariant()} {uri.AbsolutePath}{(controller != null ? $" ({controller.ToString().Singularize()})" : string.Empty)}",
                        LogKeys.InboundRequest,
                        SpanKind.Server,
                        new Span(
                            context.Request.Headers.GetValue("x-traceid").EmptyToNull() ?? context.GetCorrelationId(), // dehydrate the span infos
                            context.Request.Headers.GetValue("x-spanid"))) // dehydrate the span infos
                                                                           // TODO: get service name as operationname (servicedescriptor?)
                    .IgnoreParentSpan()
                    .SetSpanId(context.GetRequestId())
                    .WithTag(SpanTagKey.HttpMethod, context.Request.Method.ToLowerInvariant())
                    .WithTag(SpanTagKey.HttpUrl, context.Request.GetDisplayUrl())
                    .WithTag(SpanTagKey.HttpHost, uri.Port > 0 ? $"{uri.Host}:{uri.Port}" : uri.Host)
                    .WithTag(SpanTagKey.HttpPath, uri.AbsolutePath)
                    // TODO: request size? SpanTagKey.HttpRequestSize
                    .WithTag(SpanTagKey.HttpRequestId, context.GetRequestId()).Activate(this.logger))
                {
                    try
                    {
                        await this.next.Invoke(context).AnyContext();

                        scope.Span.WithTag("http.status_code", context.Response.StatusCode);
                        // TODO: response size?

                        if (context.Response.StatusCode > 399)
                        {
                            scope.Span.SetStatus(SpanStatus.Failed, $"{context.Response.StatusCode} ({ReasonPhrases.GetReasonPhrase(context.Response.StatusCode)})");
                        }
                        else
                        {
                            scope.Span.SetStatus(SpanStatus.Succeeded, $"{context.Response.StatusCode} ({ReasonPhrases.GetReasonPhrase(context.Response.StatusCode)})");
                        }
                    }
                    catch (Exception ex)
                    {
                        tracer.Fail(exception: ex);
                        throw;
                    }
                }
            }
        }
    }
}