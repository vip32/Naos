namespace Naos.ServiceExceptions.App.Web
{
    using System;
    using System.Linq;
    using System.Net;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Application;

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
                    badRequestException.ModelState.Safe()
                        .ForEach(i => details.Errors.AddOrUpdate(i.Key, i.Value.Errors.Select(e => e.ErrorMessage).ToArray()));

                    this.logger?.LogWarning($"{LogKeys.InboundResponse} [{requestId}] http request  {details.Title} [{badRequestException.GetType().PrettyName()}] {badRequestException.Message}");

                    context.Response.BadRequest(details.Title);
                    context.Response.WriteJson(details, contentType: ContentType.JSONPROBLEM);
                }
            }
        }
    }
}
