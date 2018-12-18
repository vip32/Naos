namespace Naos.Core.Common.Web
{
    using System;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public class HttpClientLogHandler : DelegatingHandler
    {
        private const string LogPrefix = "====================";
        private const string LogSuffix = "====================";
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientLogHandler"/> class.
        /// Constructs the <see cref="HttpClientLogHandler"/> with the default <see cref="HttpClientHandler"/> and the default <see cref="DebugLogger"/>
        /// </summary>
        public HttpClientLogHandler()
            : this(new HttpClientHandler(), null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientLogHandler"/> class.
        /// Constructs the <see cref="HttpClientLogHandler"/> with a custom <see cref="HttpMessageHandler"/> and the default <see cref="DebugLogger"/>
        /// </summary>
        /// <param name="handler">User defined <see cref="HttpMessageHandler"/></param>
        public HttpClientLogHandler(HttpMessageHandler handler)
            : this(handler, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientLogHandler"/> class.
        /// Constructs the <see cref="HttpClientLogHandler"/> with a custom <see cref="ILogger"/> and the default <see cref="HttpClientHandler"/>
        /// </summary>
        /// <param name="logger">User defined <see cref="ILogger"/></param>
        public HttpClientLogHandler(ILogger logger)
            : this(new HttpClientHandler(), logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientLogHandler"/> class.
        /// Constructs the <see cref="HttpClientLogHandler"/> with a custom <see cref="ILogger"/> and a custom <see cref="HttpMessageHandler"/>
        /// </summary>
        /// <param name="handler">User defined <see cref="HttpMessageHandler"/></param>
        /// <param name="logger">User defined <see cref="ILogger"/></param>
        public HttpClientLogHandler(HttpMessageHandler handler, ILogger logger)
        {
            this.InnerHandler = handler;
            this.logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                await this.LogHttpRequest(request);
                var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
                await this.LogHttpResponse(response);
                return response;
            }
            catch (Exception ex)
            {
                this.LogHttpException(request, ex);
                throw;
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

        protected void LogHttpException(HttpRequestMessage request, Exception exception)
        {
            var message = $@"{LogPrefix} HTTP EXCEPTION: [{request.Method}] {LogSuffix}
[{request.Method}] {request.RequestUri}
{exception}";
            this.WriteLog(message, exception, LogLevel.Error);
        }

        protected async Task LogHttpRequest(HttpRequestMessage request)
        {
            var content = string.Empty;
            if (request?.Content != null)
            {
                content = await this.GetRequestContent(request).ConfigureAwait(false);
            }

            var message = $@"{LogPrefix} CLIENT HTTP REQUEST: [{request?.Method}] {LogSuffix}
{request?.RequestUri}
Headers:
{{
{request?.Headers.ToString().TrimEnd()}
}}
HttpRequest.Content: 
{content}";

            this.WriteLog(message: message);
        }

        protected async Task LogHttpResponse(HttpResponseMessage response)
        {
            var content = string.Empty;
            if (response?.Content != null)
            {
                content = await this.GetResponseContent(response).ConfigureAwait(false);
            }

            const string succeeded = "SUCCEEDED";
            const string failed = "FAILED";

            var responseResult = response == null ? failed : (response.IsSuccessStatusCode ? $"{succeeded}: {(int)response.StatusCode} {response.StatusCode}" : $"{failed}: {(int)response.StatusCode} {response.StatusCode}");

            var message = $@"{LogPrefix} CLIENT HTTP RESPONSE: [{responseResult}] {LogSuffix}
[{response?.RequestMessage?.Method}] {response?.RequestMessage?.RequestUri}
HttpResponse: {response}
HttpResponse.Content: 
{content}";

            this.WriteLog(message);
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
