namespace Naos.Core.Operations.App.Web
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Naos.Core.Common;
    using Naos.Core.Common.Web;

    public class RequestFileStorageMiddleware
    {
        private readonly ILogger<RequestFileStorageMiddleware> logger;
        private readonly OperationsLoggingOptions options;
        private readonly RequestDelegate next;

        public RequestFileStorageMiddleware(RequestDelegate next,
            ILogger<RequestFileStorageMiddleware> logger,
            IOptions<OperationsLoggingOptions> options)
        {
            this.next = next;
            this.logger = logger;
            this.options = options.Value ?? new OperationsLoggingOptions();
            // https://stackoverflow.com/questions/43403941/how-to-read-asp-net-core-response-body/43404745
        }

        public async Task Invoke(HttpContext context)
        {
            var correlationId = context.GetCorrelationId().Default(Guid.NewGuid().ToString().Remove("-"));
            var requestId = context.GetRequestId();
            var path = $"{correlationId}_{requestId}".TrimEnd('_');
            var contentLength = context.Request.ContentLength ?? 0;

            if (this.options.RequestFileStorageEnabled && this.options.RequestFileStorage != null)
            {
                if (context.Request.Body != null && contentLength > 0)
                {
                    context.Request.EnableBuffering(); // allow multiple reads
                    context.Request.Body.Position = 0;
                    try
                    {
                        await this.options.RequestFileStorage.SaveFileAsync($"{path}_request.txt", context.Request.Body).AnyContext();
                    }
                    catch (Exception ex)
                    {
                        // don't throw exceptions when logging
                        this.logger.LogWarning(ex, ex.Message);
                    }

                    context.Request.Body.Position = 0;
                }

                if (context.Response.Body != null)
                {
                    var body = context.Response.Body;
                    try
                    {
                        using (var stream = new MemoryStream())
                        {
                            context.Response.Body = stream;
                            await this.next(context).AnyContext();

                            if (stream.Length > 0)
                            {
                                context.Response.ContentLength = stream.Length; // for later use
                                stream.Position = 0;
                                //string responseBody = new StreamReader(stream).ReadToEnd();
                                try
                                {
                                    await this.options.RequestFileStorage.SaveFileAsync($"{path}_response.txt", stream).AnyContext();
                                }
                                catch (Exception ex)
                                {
                                    // don't throw exceptions when logging
                                    this.logger.LogWarning(ex, ex.Message);
                                }
                            }

                            stream.Position = 0;
                            await stream.CopyToAsync(body);
                        }
                    }
                    finally
                    {
                        context.Response.Body = body;
                    }
                }
                else
                {
                    await this.next(context).AnyContext();
                }
            }
            else
            {
                await this.next(context).AnyContext();
            }
        }
    }
}