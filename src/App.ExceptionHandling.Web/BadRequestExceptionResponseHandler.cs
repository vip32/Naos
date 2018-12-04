namespace Naos.Core.App.ExceptionHandling.Web
{
    using System;
    using System.Net;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Naos.Core.App.Web;
    using Naos.Core.Common;
    using Naos.Core.Common.Web;

    public class BadRequestExceptionResponseHandler : IExceptionResponseHandler
    {
        public bool CanHandle(Exception exception)
        {
            return exception is BadRequestException;
        }

        public void Handle(HttpContext context, Exception exception, string instance, bool hideDetails = false, bool jsonResponse = true)
        {
            if (exception is BadRequestException badRequestException)
            {
                if (jsonResponse)
                {
                    var details = new ValidationProblemDetails
                    {
                        Title = "Bad request",
                        Status = (int)HttpStatusCode.BadRequest,
                        Instance = instance,
                        Detail = hideDetails ? null : badRequestException.Message,
                        Type = hideDetails ? null : badRequestException.GetType().FullPrettyName(),
                    };
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.WriteJson(details, contentType: ContentType.JSONPROBLEM);
                }
            }
        }
    }
}
