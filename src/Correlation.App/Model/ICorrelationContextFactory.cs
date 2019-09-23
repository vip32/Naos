namespace Naos.RequestCorrelation.App
{
    /// <summary>
    /// A factory for creating and disposing an instance of a <see cref="CorrelationContext"/>.
    /// </summary>
    public interface ICorrelationContextFactory
    {
        /// <summary>
        /// Creates a new <see cref="CorrelationContext"/> with the correlation/request id set for the current request.
        /// </summary>
        /// <param name="correlationId">The correlation id set on the context.</param>
        /// <param name="correlationHeader">The header used to hold the correlation id.</param>
        /// <param name="requestId">The request id set on the context.</param>
        /// <param name="requestHeader">The header used to hold the request id.</param>
        /// <returns>A new instance of a <see cref="CorrelationContext"/>.</returns>
        CorrelationContext Create(string correlationId, string correlationHeader, string requestId, string requestHeader);

        /// <summary>
        /// Disposes of the <see cref="CorrelationContext"/> for the current request.
        /// </summary>
        void Dispose();
    }
}
