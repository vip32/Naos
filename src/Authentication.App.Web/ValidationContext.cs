namespace Naos.Core.Authentication.App.Web
{
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;

    public class ValidationContext : ResultContext<AuthenticationHandlerOptions>
    {
        public ValidationContext(HttpContext context, AuthenticationScheme scheme, AuthenticationHandlerOptions options)
            : base(context, scheme, options)
        {
        }
    }
}
