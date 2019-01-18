namespace Naos.Core.Authentication.App.Web
{
    using System;
    using System.Net.Http.Headers;
    using System.Security.Claims;
    using System.Text;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Naos.Core.Common;

    public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
    {
        private const string AuthorizationHeaderName = "Authorization";
        private const string BasicSchemeName = "Basic";

        // TODO: implement handler https://joonasw.net/view/creating-auth-scheme-in-aspnet-core-2#disqus_thread

        protected ApiKeyAuthenticationHandler(
            IOptionsMonitor<ApiKeyAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            this.Logger.LogInformation($"{LogEventIdentifiers.Authentication} handle apikey");

            // TODO: also allow the auth header to be sent in the querystring (for dashboard usage)
            // TODO: ignore for localhost

            if (!this.Request.Headers.ContainsKey(AuthorizationHeaderName))
            {
                //Authorization header not in request
                return AuthenticateResult.NoResult();
            }

            if (!AuthenticationHeaderValue.TryParse(this.Request.Headers[AuthorizationHeaderName], out AuthenticationHeaderValue headerValue))
            {
                //Invalid Authorization header
                return AuthenticateResult.NoResult();
            }

            if (!BasicSchemeName.Equals(headerValue.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                //Not a Basic authentication header
                return AuthenticateResult.NoResult();
            }

            byte[] headerValueBytes = Convert.FromBase64String(headerValue.Parameter);
            string userAndPassword = Encoding.UTF8.GetString(headerValueBytes);
            string[] parts = userAndPassword.Split(':');
            if (parts.Length != 2)
            {
                return AuthenticateResult.Fail("Invalid Basic authentication header");
            }

            bool isValidUser = parts[0] == this.Options.UserName && parts[1] == this.Options.Password;

            if (!isValidUser)
            {
                return AuthenticateResult.Fail("Invalid username or password");
            }

            return await Task.Run(() =>
            {
                var claims = new[] { new Claim(ClaimTypes.Name, "apiuser") };
                var identity = new ClaimsIdentity(claims, this.Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, this.Scheme.Name);

                return AuthenticateResult.Success(ticket);
            });
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            this.Response.Headers["WWW-Authenticate"] = $"Basic realm=\"{this.Options.Realm}\", charset=\"UTF-8\"";
            await base.HandleChallengeAsync(properties);
        }
    }
}
