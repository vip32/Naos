namespace Naos.Core.Operations.App.Web
{
    using Naos.Core.FileStorage.Domain;

    /// <summary>
    /// Options for operations request response logging.
    /// </summary>
    public class RequestStorageMiddlewareOptions
    {
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// The path patterns to ignore.
        /// </summary>
        public string[] PathBlackListPatterns { get; set; } =
            new[] { "/*.js", "/*.css", "/*.html", "/swagger*", "/favicon.ico", "/api/operations/*" };

        /// <summary>
        /// The optional filestorage to store the request/response details.
        /// </summary>
        public IFileStorage Storage { get; set; }
    }
}
