// net5 upgrade first

//namespace Naos.Foundation.Utilities.Xunit
//{
//    using System.Collections.Generic;
//    using System.Security.Claims;
//    using System.Text.Encodings.Web;
//    using System.Threading.Tasks;
//    using Microsoft.AspNetCore.Authentication;
//    using Microsoft.Extensions.Logging;
//    using Microsoft.Extensions.Options;

//    public class FakeAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
//    {
//        private readonly FakeAuthenticationHandlerOptions options;

//        public FakeAuthenticationHandler(
//            IOptionsMonitor<AuthenticationSchemeOptions> schemeOptions,
//            ILoggerFactory logger,
//            UrlEncoder encoder,
//            ISystemClock clock,
//            FakeAuthenticationHandlerOptions options = null)
//            : base(schemeOptions, logger, encoder, clock)
//        {
//            this.options = options ?? new FakeAuthenticationHandlerOptions();
//        }

//        public static string SchemeName => "Fake";

//        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
//        {
//            var claims = new List<Claim>();
//            if (this.options.Claims?.Count > 0)
//            {
//                foreach (var claim in this.options.Claims)
//                {
//                    claims.Add(new Claim(claim.Key, claim.Value));
//                }
//            }

//            var identity = new ClaimsIdentity(claims, this.Scheme.Name);
//            var principal = new ClaimsPrincipal(identity);
//            var ticket = new AuthenticationTicket(principal, this.Scheme.Name);

//            return await Task.FromResult(AuthenticateResult.Success(ticket)).ConfigureAwait(false);
//        }
//    }
//}
