namespace Naos.Core.App.Web.Correlation
{
    /// <summary>
    /// Provides access to per request correlation properties.
    /// </summary>
    public class CorrelationContext
    {
        public CorrelationContext(string correlationId, string header)
        {
            EnsureThat.EnsureArg.IsNotNullOrEmpty(correlationId, nameof(correlationId));
            EnsureThat.EnsureArg.IsNotNullOrEmpty(correlationId, nameof(header));

            this.CorrelationId = correlationId;
            this.Header = header;
        }

        /// <summary>
        /// The Correlation ID which is applicable to the current request.
        /// </summary>
        public string CorrelationId { get; }

        /// <summary>
        /// The name of the header from which the Correlation ID is read/written.
        /// </summary>
        public string Header { get; }
    }
}
