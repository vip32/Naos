namespace Naos.Authentication.Application.Web
{
    using System;
    using System.Linq;
    using System.Security.Claims;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Naos.Foundation;
    using Newtonsoft.Json;

    public class EasyAuthAuthenticationHandler : BaseAuthenticationHandler<AuthenticationHandlerOptions, AuthenticationHandlerEvents>
    {
        public EasyAuthAuthenticationHandler(
            IOptionsMonitor<AuthenticationHandlerOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                this.Logger.LogInformation("{LogKey:l} easyauth handle", LogKeys.Authentication);

                if (this.Request.Host.Host.SafeEquals("localhost") && this.Options.IgnoreLocal)
                {
                    // ignore for localhost
                    var identity = new ClaimsIdentity(
                        this.Options.Claims.Safe().Select(c => new Claim(c.Key, c.Value))
                        .Insert(new Claim(ClaimTypes.AuthenticationMethod, AuthenticationKeys.EasyAuthScheme))
                        .Insert(new Claim(ClaimTypes.Name, ClaimsIdentity.DefaultIssuer))
                        .DistinctBy(c => c.Type),
                        this.Scheme.Name);
                    var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), this.Scheme.Name);
                    this.Logger.LogInformation($"{{LogKey:l}} easyauth authenticated (name={identity.Name})", LogKeys.Authentication);

                    return AuthenticateResult.Success(ticket);
                }

                var isEnabled = string.Equals(Environment.GetEnvironmentVariable("WEBSITE_AUTH_ENABLED", EnvironmentVariableTarget.Process), "True", StringComparison.OrdinalIgnoreCase);
                if (!isEnabled)
                {
                    return AuthenticateResult.NoResult();
                }

                var provider = this.Context.Request.Headers["X-MS-CLIENT-PRINCIPAL-IDP"].FirstOrDefault();
                var principalEncoded = this.Context.Request.Headers["X-MS-CLIENT-PRINCIPAL"].FirstOrDefault();
                if (principalEncoded.IsNullOrEmpty())
                {
                    return AuthenticateResult.NoResult();
                }

                var principalBytes = Convert.FromBase64String(principalEncoded);
                var principalDecoded = System.Text.Encoding.Default.GetString(principalBytes);
                var clientPrincipal = JsonConvert.DeserializeObject<ClientPrincipal>(principalDecoded);

                var principal = new ClaimsPrincipal();
                var claims = clientPrincipal.Claims.Select(c => new Claim(c.Type, c.Value));
                principal.AddIdentity(
                    new ClaimsIdentity(claims, clientPrincipal.AuthenticationType, clientPrincipal.NameType, clientPrincipal.RoleType));

                this.Logger.LogInformation($"{{LogKey:l}} easyauth authenticated (name={principal.Identity.Name})", LogKeys.Authentication);

                return AuthenticateResult.Success(new AuthenticationTicket(principal, provider));
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, $"{{LogKey:l}} {ex.Message}", LogKeys.Authentication);
                var context = new ErrorContext(this.Context, this.Scheme, this.Options) { Exception = ex };
                if (this.Events != null)
                {
                    await this.Events.Error(context).AnyContext();
                    if (context.Result != null)
                    {
                        return context.Result;
                    }
                }

                throw;
            }
        }

        /// <summary>
        /// Override this method to deal with 401 challenge concerns, if an authentication scheme in question
        /// deals an authentication interaction as part of it's request flow. (like adding a response header, or
        /// changing the 401 result to 302 of a login page or external sign-in location.)
        /// </summary>
        /// <param name="properties"></param>
        /// <returns>
        /// A Task.
        /// </returns>
        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            var authResult = await this.HandleAuthenticateOnceSafeAsync().AnyContext();

            if (authResult.Succeeded)
            {
                return;
            }

            var eventContext = new ChallengeContext(this.Context, this.Scheme, this.Options, properties)
            {
                Exception = authResult.Failure
            };

            if (this.Events != null)
            {
                await this.Events.Challenge(eventContext).AnyContext();

                if (eventContext.Handled)
                {
                    return;
                }
            }

            this.Response.Headers["WWW-Authenticate"] = $"{AuthenticationKeys.EasyAuthScheme} realm=\"{this.Options.Realm}\", charset=\"UTF-8\"";
            //this.Response.StatusCode = 401; //(int)eventContext.StatusCode;
        }
    }
}
