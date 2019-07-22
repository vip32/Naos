namespace Naos.Core.Operations.App.Web
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Humanizer;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Extensions;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Naos.Foundation;
    using Naos.Foundation.Application;

    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<RequestLoggingMiddleware> logger;
        private readonly RequestLoggingMiddlewareOptions options;

        public RequestLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestLoggingMiddleware> logger,
            IOptions<RequestLoggingMiddlewareOptions> options)
        {
            this.next = next;
            this.logger = logger;
            this.options = options.Value ?? new RequestLoggingMiddlewareOptions();
        }

        public async Task Invoke(HttpContext context)
        {
            if(!this.options.Enabled
                || context.Request.Path.Value.EqualsPatternAny(this.options.PathBlackListPatterns))
            {
                await this.next.Invoke(context).AnyContext();
            }
            else
            {
                var correlationId = context.GetCorrelationId();
                var requestId = context.GetRequestId();

                await this.LogRequestAsync(context, correlationId, requestId).AnyContext();
                using(var timer = new Timer()) // alternative for timing (actionfilter) https://stackoverflow.com/questions/48822947/asp-net-core-measure-performance
                {
                    await this.next.Invoke(context).AnyContext();
                    timer.Stop();
                    await this.LogResponseAsync(context, correlationId, requestId, timer.Elapsed).AnyContext();
                }
            }
        }

        private async Task LogRequestAsync(HttpContext context, string correlationId, string requestId)
        {
            await Task.Run(() =>
            {
                var contentLength = context.Request.ContentLength ?? 0;
                object action = null;
                object controller = null;
                context.GetRouteData()?.Values.TryGetValue("Action", out action);
                context.GetRouteData()?.Values.TryGetValue("Controller", out controller);

                this.logger.LogJournal(LogKeys.InboundRequest, $"[{requestId}] http {context.Request.Method} {{Url:l}} (endpoint={$"{action ?? context.Request.Method} {(controller != null ? controller.ToString().Singularize() ?? controller : context.Request.Uri().AbsolutePath)}".ToLowerInvariant()}, size={contentLength.Bytes().ToString("#.##")})", LogPropertyKeys.TrackInboundRequest, args: new object[] { new Uri(context.Request.GetDisplayUrl()) });
                this.logger.LogTrace(LogKeys.InboundRequest, requestId, context.Request.Path, LogTraceNames.Http); // TODO: obsolete
                //if (context.HasServiceName())
                //{
                //    this.logger.LogInformation($"SERVICE [{requestId}] http request service {context.GetServiceName()}");
                //}

                if(!context.Request.Headers.IsNullOrEmpty())
                {
                    this.logger.LogInformation($"{{LogKey:l}} [{requestId}] http headers={string.Join("|", context.Request.Headers.Select(h => $"{h.Key}={h.Value}"))}", LogKeys.InboundRequest);
                }
            });
        }

        private async Task LogResponseAsync(HttpContext context, string correlationId, string requestId, TimeSpan duration)
        {
            await Task.Run(() =>
            {
                var level = LogLevel.Information;
                if(context.Response.StatusCode > 499)
                {
                    level = LogLevel.Error;
                }
                else if(context.Response.StatusCode > 399)
                {
                    level = LogLevel.Warning;
                }

                if(!context.Response.Headers.IsNullOrEmpty())
                {
                    this.logger.Log(level, $"{{LogKey:l}} [{requestId}] http headers={string.Join("|", context.Response.Headers.Select(h => $"{h.Key}={h.Value}"))}", LogKeys.InboundResponse);
                }

                var contentLength = context.Response.ContentLength ?? 0;
                this.logger.LogJournal(LogKeys.InboundResponse, $"[{requestId}] http {context.Request.Method} {{Url:l}} {{StatusCode}} ({ReasonPhrases.GetReasonPhrase(context.Response.StatusCode)}) -> took {duration.Humanize()} (size={contentLength.Bytes().ToString("#.##")})", LogPropertyKeys.TrackInboundResponse, duration, args: new object[] { new Uri(context.Request.GetDisplayUrl()), context.Response.StatusCode });
                this.logger.LogTrace(LogKeys.InboundResponse, requestId, context.Request.Path, LogTraceNames.Http, duration); // TODO: obsolete
            }).AnyContext();
        }
    }
}