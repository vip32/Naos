namespace Naos.Core.Commands.Exceptions.Web
{
    using System;
    using System.Net;
    using FluentValidation;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Common.Web;

    public class FluentValidationExceptionResponseHandler : IExceptionResponseHandler
    {
        private readonly ILogger<FluentValidationExceptionResponseHandler> logger;

        public FluentValidationExceptionResponseHandler(ILogger<FluentValidationExceptionResponseHandler> logger)
        {
            this.logger = logger;
        }

        public bool CanHandle(Exception exception)
        {
            return exception is ValidationException;
        }

        public void Handle(HttpContext context, Exception exception, string instance, string requestId, bool hideDetails = false, bool jsonResponse = true)
        {
            if (exception is ValidationException validationException)
            {
                if (jsonResponse)
                {
                    var details = new ValidationProblemDetails
                    {
                        Title = "A model validation error has occurred while executing the request",
                        Status = (int)HttpStatusCode.BadRequest,
                        Instance = instance,
                        Detail = hideDetails ? null : validationException.Message,
                        Type = hideDetails ? null : validationException.GetType().FullPrettyName(),
                    };
                    validationException.Errors.Safe()
                        .ForEach(e => details.Errors.Add(e.PropertyName, new[] { e.ToString() }));

                    this.logger?.LogWarning($"{LogEventKeys.InboundResponse} [{requestId}] http request {details.Title} [{validationException.GetType().PrettyName()}] {validationException.Message}");

                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.WriteJson(details, contentType: ContentType.JSONPROBLEM);
                }
            }
        }
    }
}
