namespace Naos.Operations.Application.Web
{
    /// <summary>
    /// Options for operations request response logging.
    /// </summary>
    public class RequestLoggingMiddlewareOptions
    {
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// The path patterns to ignore.
        /// </summary>
        public string[] PathBlackListPatterns { get; set; } =
            new[] { "/*.js", "/*.css", "/*.html", "/swagger*", "/favicon.ico", "/naos/operations/*" };
    }
}
