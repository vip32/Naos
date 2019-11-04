namespace Naos.ServiceExceptions.Application.Web
{
    using System;
    using Microsoft.AspNetCore.Http;

    public interface IExceptionResponseHandler
    {
        bool CanHandle(Exception exception);

        void Handle(HttpContext context, Exception exception, string instance, string requestId, bool hideDetails = false, bool jsonResponse = true);
    }
}
