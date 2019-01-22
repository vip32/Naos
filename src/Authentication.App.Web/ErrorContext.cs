namespace Naos.Core.Authentication.App.Web
{
    using System;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;

    public class ErrorContext : ResultContext<AuthenticationHandlerOptions>
    {
        public ErrorContext(HttpContext context, AuthenticationScheme scheme, AuthenticationHandlerOptions options)
            : base(context, scheme, options)
        {
        }

        public Exception Exception { get; set; }
    }
}
