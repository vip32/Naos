namespace Naos.Core.Operations.App.Web
{
    using Naos.Core.FileStorage.Domain;

    /// <summary>
    /// Options for operations request response logging.
    /// </summary>
    public class RequestResponseLoggingOptions
    {
        private const string DefaultRequestHeader = "X-RequestId";

        /// <summary>
        /// The name of the header from which the request id is read.
        /// </summary>
        public string RequestHeader { get; set; } = DefaultRequestHeader;

        /// <summary>
        /// The path patterns to ignore
        /// </summary>
        public string[] PathBlackListPatterns { get; set; } = new[] { "/swagger*", "/favicon.ico", "/api/operations/logevents*" };

        /// <summary>
        /// The optional filestorage to store the request/response details
        /// </summary>
        public IFileStorage FileStorage { get; set; }

        public bool FileStorageEnabled { get; set; } = true;
    }
}
