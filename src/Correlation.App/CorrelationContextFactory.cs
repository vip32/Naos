namespace Naos.Core.Correlation.App
{
    /// <inheritdoc />
    public class CorrelationContextFactory : ICorrelationContextFactory
    {
        private readonly ICorrelationContextAccessor accessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelationContextFactory" /> class.
        /// </summary>
        public CorrelationContextFactory()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelationContextFactory"/> class.
        /// </summary>
        /// <param name="accessor">The <see cref="ICorrelationContextAccessor"/> through which the <see cref="CorrelationContext"/> will be set.</param>
        public CorrelationContextFactory(ICorrelationContextAccessor accessor)
        {
            this.accessor = accessor;
        }

        /// <inheritdoc />
        public CorrelationContext Create(string correlationId, string correlationHeader, string requestId, string requestHeader)
        {
            var result = new CorrelationContext(correlationId, correlationHeader, requestId, requestHeader);

            if (this.accessor != null)
            {
                this.accessor.Context = result;
            }

            return result;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (this.accessor != null)
            {
                this.accessor.Context = null;
            }
        }
    }
}