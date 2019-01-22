namespace Naos.Core.Authentication.App.Web
{
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;

    public class ValidationContext : ResultContext<ApiKeyAuthenticationOptions>
    {
        public ValidationContext(HttpContext context, AuthenticationScheme scheme, ApiKeyAuthenticationOptions options)
            : base(context, scheme, options)
        {
        }

        public string ApiKey { get; set; }
    }
}
