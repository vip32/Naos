namespace Naos.Core.Operations.App.Web
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Humanizer;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Extensions;
    using Microsoft.AspNetCore.WebUtilities;
    //using Microsoft.AspNetCore.Http.Internal;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.IO;
    using Naos.Core.Common;
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
            if (context.Request.Path.Value.EqualsPatternAny(this.options.PathBlackListPatterns))
            {
                await this.next.Invoke(context).AnyContext();
            }
            else
            {
                var correlationId = context.GetCorrelationId();
                var requestId = context.GetRequestId();

                await this.LogRequestAsync(context, correlationId, requestId).AnyContext();
                var timer = Stopwatch.StartNew();
                await this.next.Invoke(context).AnyContext();
                timer.Stop();
                await this.LogResponseAsync(context, correlationId, requestId, timer.Elapsed).AnyContext();
            }
        }

        private async Task LogRequestAsync(HttpContext context, string correlationId, string requestId)
        {
            this.logger.LogJournal(LogEventPropertyKeys.TrackInboundRequest, $"{{LogKey:l}} [{requestId}] http {context.Request.Method} {{Url:l}} ({correlationId})", args: new object[] { LogEventKeys.InboundRequest, new Uri(context.Request.GetDisplayUrl()) });

            //if (context.HasServiceName())
            //{
            //    this.logger.LogInformation($"SERVICE [{requestId}] http request service {context.GetServiceName()}");
            //}

            if (!context.Request.Headers.IsNullOrEmpty())
            {
                this.logger.LogInformation($"{{LogKey:l}} [{requestId}] http headers={string.Join("|", context.Request.Headers.Select(h => $"{h.Key}={h.Value}"))}", LogEventKeys.InboundRequest);
            }

            if (this.options.FileStorageEnabled && this.options.FileStorage != null && context.Request.Body != null)
            {
                context.Request.EnableBuffering(); // allow multiple reads
                if (context.Request.Body.Length > 0)
                {
                    await this.options.FileStorage.SaveFileAsync($"{correlationId}_{requestId}_request.txt", context.Request.Body).AnyContext();
                    context.Request.Body.Position = 0;
                }
            }

            //request.EnableRewind();
            //using (var stream = this.streamManager.GetStream()) // https://stackoverflow.com/questions/43403941/how-to-read-asp-net-core-response-body/43404745
            //{
            //    request.Body.CopyTo(stream);

            //    this.logger.LogInformation($"Http Request Information:{System.Environment.NewLine}" +
            //                           $"Schema:{request.Scheme} " +
            //                           $"Host: {request.Host} " +
            //                           $"Path: {request.Path} " +
            //                           $"QueryString: {request.QueryString} " +
            //                           $"Request Body: {ReadStreamInChunks(stream)}");
            //}

            // OR log json request https://www.strathweb.com/2019/02/be-careful-when-manually-handling-json-requests-in-asp-net-core/
        }

        private async Task LogResponseAsync(HttpContext context, string correlationId, string requestId, TimeSpan elapsed)
        {
            await Task.Run(() =>
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

                if (!context.Response.Headers.IsNullOrEmpty())
                {
                    this.logger.Log(level, $"{{LogKey:l}} [{requestId}] http headers={string.Join("|", context.Response.Headers.Select(h => $"{h.Key}={h.Value}"))}", LogEventKeys.InboundResponse);
                }

                this.logger.LogJournal(LogEventPropertyKeys.TrackInboundResponse, $"{{LogKey:l}} [{requestId}] http {context.Request.Method} {{Url:l}} {{StatusCode}} ({ReasonPhrases.GetReasonPhrase(context.Response.StatusCode)}) -> took {elapsed.Humanize(3)}", level, args: new object[] { LogEventKeys.InboundResponse, new Uri(context.Request.GetDisplayUrl()), context.Response.StatusCode });
            }).AnyContext();
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
        //                           $"Response Body: {this.ReadStreamInChunks(stream)}");
        //}

        //context.Response.Body = body;
        //}

        //private string ReadStreamInChunks(Stream stream)
        //{
        //    string result;
        //    stream.Seek(0, SeekOrigin.Begin);

        //    using (var textWriter = new StringWriter())
        //    using (var reader = new StreamReader(stream))
        //    {
        //        var readChunk = new char[ReadChunkBufferLength];
        //        int readChunkLength;

        //        do //do while: is useful for the last iteration in case readChunkLength < chunkLength
        //        {
        //            readChunkLength = reader.ReadBlock(readChunk, 0, ReadChunkBufferLength);
        //            textWriter.Write(readChunk, 0, readChunkLength);
        //        }
        //        while (readChunkLength > 0);

        //        result = textWriter.ToString();
        //    }

        //    return result;
        //}
    }
}