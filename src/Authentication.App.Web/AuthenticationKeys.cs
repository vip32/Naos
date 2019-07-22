namespace Naos.Core.Authentication.App.Web
{
    /// <summary>
    /// Default values used by API key authentication.
    /// </summary>
    public struct AuthenticationKeys
    {
        /// <summary>
        /// Default value for AuthenticationScheme.
        /// </summary>
        public const string ApiKeyScheme = "ApiKey";

        public const string BasicScheme = "Basic";

        public const string EasyAuthScheme = "EasyAuth";

        public const string AuthorizationHeaderName = "Authorization";
    }
}
