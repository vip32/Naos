namespace Naos.Core.App.ExceptionHandling.Web
{
    using System;
    using System.Net;
    using FluentValidation;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Naos.Core.Common;
    using Naos.Core.Common.Web;

    public class FluentValidationExceptionResponseHandler : IExceptionResponseHandler
    {
        public bool CanHandle(Exception exception)
        {
            return exception is ValidationException;
        }

        public void Handle(HttpContext context, Exception exception, string instance, bool hideDetails = false, bool jsonResponse = true)
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
                    validationException.Errors.NullToEmpty().ForEach(f => details.Errors.Add(f.PropertyName, new[] { f.ToString() }));
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.WriteJson(details, contentType: ContentType.JSONPROBLEM);
                }
            }
        }
    }
}
