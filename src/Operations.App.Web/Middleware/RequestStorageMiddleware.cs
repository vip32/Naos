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

    public class RequestStorageMiddleware
    {
        private readonly ILogger<RequestStorageMiddleware> logger;
        private readonly RequestStorageMiddlewareOptions options;
        private readonly RequestDelegate next;

        public RequestStorageMiddleware(RequestDelegate next,
            ILogger<RequestStorageMiddleware> logger,
            IOptions<RequestStorageMiddlewareOptions> options)
        {
            this.next = next;
            this.logger = logger;
            this.options = options.Value ?? new RequestStorageMiddlewareOptions();
            // https://stackoverflow.com/questions/43403941/how-to-read-asp-net-core-response-body/43404745
        }

        public async Task Invoke(HttpContext context)
        {
            if(!this.options.Enabled
                || this.options.Storage == null
                || context.Request.Path.Value.EqualsPatternAny(this.options.PathBlackListPatterns))
            {
                await this.next.Invoke(context).AnyContext();
            }
            else
            {
                await this.LogRequestAsync(context);
            }
        }

        private async Task LogRequestAsync(HttpContext context)
        {
            var correlationId = context.GetCorrelationId().Default(Guid.NewGuid().ToString().Remove("-"));
            var requestId = context.GetRequestId();
            var path = $"{correlationId}_{requestId}".TrimEnd('_');
            var contentLength = context.Request.ContentLength ?? 0;

            if(context.Request.Body != null && contentLength > 0)
            {
                context.Request.EnableBuffering(); // allow multiple reads
                context.Request.Body.Position = 0;
                try
                {
                    await this.options.Storage.SaveFileAsync($"{path}_request.txt", context.Request.Body).AnyContext();
                }
                catch(Exception ex)
                {
                    // don't throw exceptions when logging
                    this.logger.LogWarning(ex, ex.Message);
                }

                context.Request.Body.Position = 0;
            }

            if(context.Response.Body != null)
            {
                var body = context.Response.Body;
                try
                {
                    using(var stream = new MemoryStream())
                    {
                        context.Response.Body = stream;
                        await this.next(context).AnyContext();

                        if(stream.Length > 0)
                        {
                            context.Response.ContentLength = stream.Length; // for later use
                            stream.Position = 0;
                            //string responseBody = new StreamReader(stream).ReadToEnd();
                            try
                            {
                                await this.options.Storage.SaveFileAsync($"{path}_response.txt", stream).AnyContext();
                            }
                            catch(Exception ex)
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
    }
}