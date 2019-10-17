namespace Naos.Tracing.App.Web
{
    /// <summary>
    /// Options for operations request response tracing.
    /// </summary>
    public class RequestTracingMiddlewareOptions
    {
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// The path patterns to ignore.
        /// </summary>
        public string[] PathBlackListPatterns { get; set; } =
            new[] { "/*.js", "/*.css", "/*.html", "/swagger*", "/favicon.ico", "/api/operations/*" };
    }
}
