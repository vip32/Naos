namespace Naos.Core.App.Operations.Web
{
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;
    using Humanizer;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Extensions;
    //using Microsoft.AspNetCore.Http.Internal;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.IO;
    using Naos.Core.Common.Web;

    public class RequestResponseLoggingMiddleware
    {
        private const int ReadChunkBufferLength = 4096;
        private readonly RequestDelegate next;
        private readonly ILogger<RequestResponseLoggingMiddleware> logger;
        private readonly RequestResponseLoggingOptions options;
        private readonly RecyclableMemoryStreamManager streamManager;

        public RequestResponseLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestResponseLoggingMiddleware> logger,
            IOptions<RequestResponseLoggingOptions> options)
        {
            this.next = next;
            this.logger = logger;
            this.options = options.Value ?? new RequestResponseLoggingOptions();
            this.streamManager = new RecyclableMemoryStreamManager();
        }

        public async Task Invoke(HttpContext context)
        {
            var requestId = context.GetRequestId();

            this.LogRequest(context, requestId);
            var timer = Stopwatch.StartNew();
            await this.next.Invoke(context).ConfigureAwait(false);
            timer.Stop();
            this.LogResponse(context, requestId, timer.Elapsed);
        }

        private static string ReadStreamInChunks(Stream stream)
        {
            string result;
            stream.Seek(0, SeekOrigin.Begin);

            using (var textWriter = new StringWriter())
            using (var reader = new StreamReader(stream))
            {
                var readChunk = new char[ReadChunkBufferLength];
                int readChunkLength;

                do //do while: is useful for the last iteration in case readChunkLength < chunkLength
                {
                    readChunkLength = reader.ReadBlock(readChunk, 0, ReadChunkBufferLength);
                    textWriter.Write(readChunk, 0, readChunkLength);
                }
                while (readChunkLength > 0);

                result = textWriter.ToString();
            }

            return result;
        }

        private void LogRequest(HttpContext context, string requestId)
        {
            this.logger.LogInformation($"SERVICE http request  ({{RequestId}}) {context.Request.Method} {context.Request.GetDisplayUrl()}", requestId);

            //request.EnableRewind();
            //using (var stream = this.streamManager.GetStream())
            //{
            //    request.Body.CopyTo(stream);

            //    this.logger.LogInformation($"Http Request Information:{System.Environment.NewLine}" +
            //                           $"Schema:{request.Scheme} " +
            //                           $"Host: {request.Host} " +
            //                           $"Path: {request.Path} " +
            //                           $"QueryString: {request.QueryString} " +
            //                           $"Request Body: {ReadStreamInChunks(stream)}");
            //}
        }

        private void LogResponse(HttpContext context, string requestId, System.TimeSpan elapsed)
        {
            var level = LogLevel.Information;
            if ((int)context.Response.StatusCode > 499)
            {
                level = LogLevel.Error;
            }
            else if ((int)context.Response.StatusCode > 399)
            {
                level = LogLevel.Warning;
            }

            this.logger.Log(level, $"SERVICE http response ({{RequestId}}) {context.Request.Method} {context.Request.GetDisplayUrl()} {context.Response.StatusCode} -> took {elapsed.Humanize(3)}", requestId);
        }

        //private async Task LogResponseAsync(HttpContext context, string requestId)
        //{
            //var body = context.Response.Body;
            //using (var stream = this.streamManager.GetStream())
            //{
            //    context.Response.Body = stream;

            //    await this.next.Invoke(context);

            //    await stream.CopyToAsync(body);

            //    this.logger.LogInformation($"Http Response Information:{System.Environment.NewLine}" +
            //                           $"Schema:{context.Request.Scheme} " +
            //                           $"Host: {context.Request.Host} " +
            //                           $"Path: {context.Request.Path} " +
            //                           $"QueryString: {context.Request.QueryString} " +
            //                           $"Response Body: {ReadStreamInChunks(stream)}");
            //}

            //context.Response.Body = body;
        //}
    }
}
