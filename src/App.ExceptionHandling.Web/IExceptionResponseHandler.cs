namespace Naos.Core.App.ExceptionHandling.Web
{
    using System;
    using Microsoft.AspNetCore.Http;

    public interface IExceptionResponseHandler
    {
        bool CanHandle(Exception exception);

        void Handle(HttpContext context, Exception exception, string instance, bool hideDetails = false, bool jsonResponse = true);
    }
}
