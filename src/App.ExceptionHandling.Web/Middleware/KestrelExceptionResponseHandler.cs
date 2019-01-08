namespace Naos.Core.App.Exceptions.Web
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Server.Kestrel.Core;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Common.Web;

    public class KestrelExceptionResponseHandler : IExceptionResponseHandler
    {
        private readonly ILogger<KestrelExceptionResponseHandler> logger;

        public KestrelExceptionResponseHandler(ILogger<KestrelExceptionResponseHandler> logger)
        {
            this.logger = logger;
        }

        public bool CanHandle(Exception exception)
        {
            return exception is BadHttpRequestException;
        }

        public void Handle(HttpContext context, Exception exception, string instance, string requestId, bool hideDetails = false, bool jsonResponse = true)
        {
            if (exception is BadHttpRequestException badHttpRequestException)
            {
                if (jsonResponse)
                {
                    // low level kestrel exception (too large paylod, method not allowed, too large headers)
                    var details = new ProblemDetails
                    {
                        Title = "Invalid request",
                        Status = (int)typeof(BadHttpRequestException).GetProperty("StatusCode", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(exception),
                        Instance = instance,
                        Detail = hideDetails ? null : badHttpRequestException.Demystify().ToString(),
                        Type = hideDetails ? null : badHttpRequestException.GetType().FullPrettyName()
                    };

                    this.logger?.LogWarning(exception, $"SERVICE http request  ({{RequestId}}) {details.Title} [{badHttpRequestException.GetType().PrettyName()}] {badHttpRequestException.Message}", requestId);

                    context.Response.StatusCode = details.Status.Value;
                    context.Response.WriteJson(details, contentType: ContentType.JSONPROBLEM);
                }
            }
        }
    }
}
