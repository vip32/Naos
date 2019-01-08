namespace Naos.Core.Correlation.App
{
    /// <summary>
    /// Provides access to the request correlation properties.
    /// </summary>
    public class CorrelationContext
    {
        public CorrelationContext(string correlationId, string correlationHeader, string requestId, string requestHeader)
        {
            this.CorrelationId = correlationId;
            this.CorrelationHeader = correlationHeader;
            this.RequestId = requestId;
            this.RequestHeader = requestHeader;
        }

        /// <summary>
        /// The correlation id which is applicable to the current request.
        /// </summary>
        public string CorrelationId { get; }

        /// <summary>
        /// The name of the header from which the correlation id is read/written.
        /// </summary>
        public string CorrelationHeader { get; }

        /// <summary>
        /// The request id which is applicable to the current request.
        /// </summary>
        public string RequestId { get; }

        /// <summary>
        /// The name of the header from which the request id is read/written.
        /// </summary>
        public string RequestHeader { get; }
    }
}
