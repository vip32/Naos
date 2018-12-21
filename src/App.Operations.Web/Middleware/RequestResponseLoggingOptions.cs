namespace Naos.Core.App.Operations.Web
{
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
    }
}
