namespace Naos.Core.Authentication.App.Web
{
    using System;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;

    public class ErrorContext : ResultContext<ApiKeyAuthenticationOptions>
    {
        public ErrorContext(HttpContext context, AuthenticationScheme scheme, ApiKeyAuthenticationOptions options)
            : base(context, scheme, options)
        {
        }

        public Exception Exception { get; set; }
    }
}
