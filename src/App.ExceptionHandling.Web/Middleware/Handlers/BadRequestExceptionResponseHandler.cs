namespace Naos.Core.Commands.Exceptions.Web
{
    using System;
    using System.Linq;
    using System.Net;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Common.Web;

    public class BadRequestExceptionResponseHandler : IExceptionResponseHandler
    {
        private readonly ILogger<BadRequestExceptionResponseHandler> logger;

        public BadRequestExceptionResponseHandler(ILogger<BadRequestExceptionResponseHandler> logger)
        {
            this.logger = logger;
        }

        public bool CanHandle(Exception exception)
        {
            return exception is BadRequestException;
        }

        public void Handle(HttpContext context, Exception exception, string instance, string requestId, bool hideDetails = false, bool jsonResponse = true)
        {
            if (exception is BadRequestException badRequestException)
            {
                if (jsonResponse)
                {
                    var details = new ValidationProblemDetails
                    {
                        Title = badRequestException.ModelState.IsNullOrEmpty() ? "A request validation error has occurred while executing the request" : "A model validation error has occurred while executing the request",
                        Status = (int)HttpStatusCode.BadRequest,
                        Instance = instance,
                        Detail = hideDetails ? null : !badRequestException.ModelState.IsNullOrEmpty() ? "See errors property for more details" : badRequestException.Message,
                        Type = hideDetails ? null : badRequestException.GetType().FullPrettyName(),
                    };

                    foreach(var item in badRequestException.ModelState.Safe())
                    {
                        details.Errors.Add(item.Key, item.Value.Errors.Select(e => e.ErrorMessage).ToArray());
                    }

                    this.logger?.LogWarning($"{LogEventKeys.OutboundResponse} [{requestId}] http request  {details.Title} [{badRequestException.GetType().PrettyName()}] {badRequestException.Message}");

                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.WriteJson(details, contentType: ContentType.JSONPROBLEM);
                }
            }
        }
    }
}
