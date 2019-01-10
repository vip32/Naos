namespace Naos.Core.Common.Web
{
    using System;
    using System.Diagnostics;
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
            var requestId = request.GetRequestId();

            await this.LogHttpRequest(request, requestId);

            var timer = Stopwatch.StartNew();
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
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

        protected async Task LogHttpRequest(HttpRequestMessage request, string requestId)
        {
            var content = string.Empty;
            if (request?.Content != null)
            {
                content = await this.GetRequestContent(request).ConfigureAwait(false);
            }

            var message = $"CLIENT http request  ({requestId}): [{request?.Method}] {request.RequestUri}";

            this.WriteLog(message: message);
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
                content = await this.GetResponseContent(response).ConfigureAwait(false);
            }

            var message = $"CLIENT http response ({requestId}): [{response.RequestMessage.Method}] {response.RequestMessage.RequestUri} {(int)response.StatusCode} ({response.StatusCode}) -> took {elapsed.Humanize(3)}";

            this.WriteLog(message, null, level);
        }

        private void WriteLog(string message, Exception exception = null, LogLevel level = LogLevel.Information)
        {
            if (this.logger == null)
            {
                Debug.WriteLine(message);
            }
            else
            {
                // TODO: add scope (correlationid/requestid/servicedescr)
                this.logger.Log(level, exception, message);
            }
        }
    }
}
