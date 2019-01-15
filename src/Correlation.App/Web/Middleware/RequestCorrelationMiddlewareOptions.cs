namespace Naos.Core.App.Correlation.App.Web
{
    /// <summary>
    /// Options for correlation.
    /// </summary>
    public class RequestCorrelationMiddlewareOptions
    {
        private const string DefaultCorrelationHeader = "X-CorrelationId";
        private const string DefaultCorrelationLogPropertyName = "CorrelationId";
        private const string DefaultRequestHeader = "X-RequestId";
        private const string DefaultRequestLogPropertyName = "RequestId";

        /// <summary>
        /// The name of the header from which the correlation id is read/written.
        /// </summary>
        public string CorrelationHeader { get; set; } = DefaultCorrelationHeader;

        public string CorrelationLogPropertyName { get; set; } = DefaultCorrelationLogPropertyName;

        /// <summary>
        /// The name of the header from which the request id is read/written.
        /// </summary>
        public string RequestHeader { get; set; } = DefaultRequestHeader;

        public string RequestIdLogPropertyName { get; set; } = DefaultRequestLogPropertyName;

        /// <summary>
        /// Controls whether the correlation/request id is returned in the response headers.
        /// </summary>
        public bool IncludeInResponse { get; set; } = true;

        /// <summary>
        /// Controls whether the ASP.NET TraceIdentifier will be set to match the CorrelationId.
        /// </summary>
        public bool UpdateTraceIdentifier { get; set; } = true;

        /// <summary>
        /// Controls whether a GUID will be used in cases where no correlation ID is retrieved from the request header.
        /// When false the TraceIdentifier for the current request will be used.
        /// </summary>
        public bool UseGuidAsCorrelationId { get; set; } = true;

        /// <summary>
        /// Controls whether a hash will be used in cases where no correlation ID is retrieved from the request header.
        /// When false the TraceIdentifier for the current request will be hashed and used.
        /// </summary>
        public bool UseHashAsCorrelationId { get; set; }

        /// <summary>
        /// Controls the length of the request id if it could not be retrieved from the request header.
        /// </summary>
        public int RequestIdLength { get; set; } = 5;

        /// <summary>
        /// Controls if the generated request id contains alphanumeric chharacters it could not be retrieved from the request header.
        /// </summary>
        public bool RequestIdAlphanumeric { get; set; }
    }
}
