namespace Naos.Core.Common.Web
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Humanizer;
    using Microsoft.Extensions.Logging;

    public class HttpClientLogHandler : DelegatingHandler
    {
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientLogHandler"/> class.
        /// Constructs the <see cref="HttpClientLogHandler"/> with a custom <see cref="ILogger"/> and the default <see cref="HttpClientHandler"/>.
        /// </summary>
        /// <param name="logger">User defined <see cref="ILogger"/>.</param>
        public HttpClientLogHandler(ILogger logger)
        {
            this.logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var correlationId = request.GetCorrelationId();
            var requestId = request.GetRequestId();

            await this.LogHttpRequest(request, correlationId, requestId);

            using(var timer = new Common.Timer())
            {
                var response = await base.SendAsync(request, cancellationToken).AnyContext();
                timer.Stop();
                await this.LogHttpResponse(response, requestId, timer.Elapsed);
                return response;
            }
        }

        protected async Task<string> GetRequestContent(HttpRequestMessage request)
        {
            return await request.Content.ReadAsStringAsync();
        }

        protected async Task<string> GetResponseContent(HttpResponseMessage response)
        {
            return await response.Content.ReadAsStringAsync();
        }

        protected async Task LogHttpRequest(HttpRequestMessage request, string correlationId, string requestId)
        {
            var content = string.Empty;
            if(request?.Content != null)
            {
                content = await this.GetRequestContent(request).AnyContext();
            }

            if(!request.Headers.IsNullOrEmpty())
            {
                this.WriteLog(LogKeys.OutboundRequest, $"[{requestId}] http headers={string.Join("|", request.Headers.Select(h => $"{h.Key}={string.Join("|", h.Value)}"))}");
            }

            this.WriteLog(LogKeys.OutboundRequest, $"[{requestId}] http {request?.Method} {{Url:l}} ({correlationId})", type: LogEventPropertyKeys.TrackOutboundRequest, args: new object[] { request.RequestUri });
            this.logger.LogTraceEvent(LogKeys.OutboundRequest, requestId, request.RequestUri.PathAndQuery.SliceTill("?"), LogTraceEventNames.Http);
        }

        protected async Task LogHttpResponse(HttpResponseMessage response, string requestId, TimeSpan duration)
        {
            var level = LogLevel.Information;
            if((int)response.StatusCode > 499)
            {
                level = LogLevel.Error;
            }
            else if((int)response.StatusCode > 399)
            {
                level = LogLevel.Warning;
            }

            var content = string.Empty;
            if(response?.Content != null)
            {
                content = await this.GetResponseContent(response).AnyContext();
            }

            if(!response.Headers.IsNullOrEmpty())
            {
                this.WriteLog(LogKeys.OutboundResponse, $"[{requestId}] http headers={string.Join("|", response.Headers.Select(h => $"{h.Key}={string.Join("|", h.Value)}"))}", level: level);
            }

            this.WriteLog(LogKeys.OutboundResponse, $"[{requestId}] http {response.RequestMessage.Method} {{Url:l}} {{StatusCode}} ({response.StatusCode}) -> took {duration.Humanize(3)}", type: LogEventPropertyKeys.TrackOutboundResponse, duration: duration, args: new object[] { response.RequestMessage.RequestUri, (int)response.StatusCode });
            this.logger.LogTraceEvent(LogKeys.OutboundResponse, requestId, response.RequestMessage.RequestUri.PathAndQuery.SliceTill("?"), LogTraceEventNames.Http, duration);
        }

        private void WriteLog(
            string logKey,
            string message,
            Exception exception = null,
            LogLevel level = LogLevel.Information,
            string type = null,
            TimeSpan? duration = null,
            IDictionary<string, object> properties = null,
            params object[] args)
        {
            if(this.logger == null)
            {
                Debug.WriteLine(message);
            }
            else
            {
                if(type.IsNullOrEmpty())
                {
                    this.logger.Log(level, exception, $"{logKey:l} {message:l}", args);
                }
                else
                {
                    this.logger.LogJournal(logKey, message, type, duration, properties, args);
                }
            }
        }
    }
}
