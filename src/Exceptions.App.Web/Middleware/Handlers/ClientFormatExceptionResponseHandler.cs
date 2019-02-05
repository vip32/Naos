namespace Naos.Core.Commands.Exceptions.Web
{
    using System;
    using System.Net;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Common.Web;

    public class ClientFormatExceptionResponseHandler : IExceptionResponseHandler
    {
        private readonly ILogger<ClientFormatExceptionResponseHandler> logger;

        public ClientFormatExceptionResponseHandler(ILogger<ClientFormatExceptionResponseHandler> logger)
        {
            this.logger = logger;
        }

        public bool CanHandle(Exception exception)
        {
            return exception is NaosClientFormatException;
        }

        public void Handle(HttpContext context, Exception exception, string instance, string requestId, bool hideDetails = false, bool jsonResponse = true)
        {
            if (exception is NaosClientFormatException formatException)
            {
                if (jsonResponse)
                {
                    var details = new ValidationProblemDetails
                    {
                        Title = "A request content client format error has occurred while executing the request",
                        Status = (int)HttpStatusCode.BadRequest,
                        Instance = instance,
                        Detail = hideDetails ? null : formatException.Message.SubstringTill("\r\n"),
                        Type = hideDetails ? null : formatException.GetType().FullPrettyName(),
                    };

                    this.logger?.LogWarning($"{LogEventKeys.InboundResponse} [{requestId}] http request  {details.Title} [{formatException.GetType().PrettyName()}] {formatException.Message.SubstringTill("\r\n")}");

                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.WriteJson(details, contentType: ContentType.JSONPROBLEM);
                }
            }
        }
    }
}
