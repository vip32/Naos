namespace Naos.Authentication.Application.Web
{
    /// <summary>
    /// Options for authenctication challenge.
    /// </summary>
    public class OidcAuthenticationChallengeMiddlewareOptions
    {
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// The path patterns to ignore.
        /// </summary>
        public string[] PathBlackListPatterns { get; set; } =
            new[] { "/*.js", "/*.css", "/*.map", "/*.html", "/swagger*", "/favicon.ico", "/signin-oidc", "/health" };
    }
}
