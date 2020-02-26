namespace Naos.Tracing.Application.Web
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
            new[] { "/*.js", "/*.css", "/*.map", "/*.html", "/swagger*", "/favicon.ico", "/naos/operations/*" };
    }
}
