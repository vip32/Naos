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

    public class RequestResponseFileStorageMiddleware
    {
        private readonly ILogger<RequestResponseFileStorageMiddleware> logger;
        private readonly RequestResponseLoggingOptions options;
        private readonly RequestDelegate next;

        public RequestResponseFileStorageMiddleware(RequestDelegate next,
            ILogger<RequestResponseFileStorageMiddleware> logger,
            IOptions<RequestResponseLoggingOptions> options)
        {
            this.next = next;
            this.logger = logger;
            this.options = options.Value ?? new RequestResponseLoggingOptions();
        }

        public async Task Invoke(HttpContext context)
        {
            var correlationId = context.GetCorrelationId().Default(Guid.NewGuid().ToString().Remove("-"));
            var requestId = context.GetRequestId();
            var path = $"{correlationId}_{requestId}".TrimEnd('_');

            if (this.options.FileStorageEnabled && this.options.FileStorage != null)
            {
                if (context.Request.Body != null)
                {
                    context.Request.EnableBuffering(); // allow multiple reads
                    if (context.Request.Body.Length > 0)
                    {
                        try
                        {
                            await this.options.FileStorage.SaveFileAsync($"{path}_request.txt", context.Request.Body).AnyContext();
                        }
                        catch(Exception ex)
                        {
                            // don't throw exceptions when logging
                            this.logger.LogWarning(ex, ex.Message);
                        }

                        context.Request.Body.Position = 0;
                    }
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
                                stream.Position = 0;
                                //string responseBody = new StreamReader(stream).ReadToEnd();
                                try
                                {
                                    await this.options.FileStorage.SaveFileAsync($"{path}_response.txt", stream).AnyContext();
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