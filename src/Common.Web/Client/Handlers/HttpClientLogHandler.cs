namespace Naos.Core.Common.Web
{
    using System;
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
        /// Constructs the <see cref="HttpClientLogHandler"/> with a custom <see cref="ILogger"/> and the default <see cref="HttpClientHandler"/>
        /// </summary>
        /// <param name="logger">User defined <see cref="ILogger"/></param>
        public HttpClientLogHandler(ILogger logger)
        {
            this.logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var correlationId = request.GetCorrelationId();
            var requestId = request.GetRequestId();

            await this.LogHttpRequest(request, correlationId, requestId);

            var timer = Stopwatch.StartNew();
            var response = await base.SendAsync(request, cancellationToken).AnyContext();
            timer.Stop();

            await this.LogHttpResponse(response, requestId, timer.Elapsed);
            return response;
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
            if (request?.Content != null)
            {
                content = await this.GetRequestContent(request).AnyContext();
            }

            if (!request.Headers.IsNullOrEmpty())
            {
                this.WriteLog($"{{LogKey:l}} [{requestId}] http headers={string.Join("|", request.Headers.Select(h => $"{h.Key}={string.Join("|", h.Value)}"))}", args: LogEventKeys.OutboundRequest);
            }

            this.WriteLog($"{{LogKey:l}} [{requestId}] http {request?.Method} {{Url:l}} ({correlationId})", journalType: LogEventPropertyKeys.TrackOutboundRequest, args: new object[] { LogEventKeys.OutboundRequest, request.RequestUri });
        }

        protected async Task LogHttpResponse(HttpResponseMessage response, string requestId, TimeSpan elapsed)
        {
            var level = LogLevel.Information;
            if ((int)response.StatusCode > 499)
            {
                level = LogLevel.Error;
            }
            else if ((int)response.StatusCode > 399)
            {
                level = LogLevel.Warning;
            }

            var content = string.Empty;
            if (response?.Content != null)
            {
                content = await this.GetResponseContent(response).AnyContext();
            }

            if (!response.Headers.IsNullOrEmpty())
            {
                this.WriteLog($"{{LogKey:l}} [{requestId}] http headers={string.Join("|", response.Headers.Select(h => $"{h.Key}={string.Join("|", h.Value)}"))}", level: level, args: LogEventKeys.OutboundResponse);
            }

            this.WriteLog($"{{LogKey:l}} [{requestId}] http {response.RequestMessage.Method} {{Url:l}} {{StatusCode}} ({response.StatusCode}) -> took {elapsed.Humanize(3)}", null, level, journalType: LogEventPropertyKeys.TrackOutboundResponse, args: new object[] { LogEventKeys.OutboundResponse, response.RequestMessage.RequestUri, (int)response.StatusCode});
        }

        private void WriteLog(string message, Exception exception = null, LogLevel level = LogLevel.Information, string journalType = null, params object[] args)
        {
            if (this.logger == null)
            {
                Debug.WriteLine(message);
            }
            else
            {
                if (journalType.IsNullOrEmpty())
                {
                    this.logger.Log(level, exception, message, args);
                }
                else
                {
                    this.logger.LogJournal(journalType, message, level, args);
                }
            }
        }
    }
}
