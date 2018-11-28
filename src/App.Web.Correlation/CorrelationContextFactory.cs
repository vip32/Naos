namespace Naos.Core.App.Web.Correlation
{
    /// <inheritdoc />
    public class CorrelationContextFactory : ICorrelationContextFactory
    {
        private readonly ICorrelationContextAccessor correlationContextAccessor;

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
        /// <param name="correlationContextAccessor">The <see cref="ICorrelationContextAccessor"/> through which the <see cref="CorrelationContext"/> will be set.</param>
        public CorrelationContextFactory(ICorrelationContextAccessor correlationContextAccessor)
        {
            this.correlationContextAccessor = correlationContextAccessor;
        }

        /// <inheritdoc />
        public CorrelationContext Create(string correlationId, string header)
        {
            var correlationContext = new CorrelationContext(correlationId, header);

            if (this.correlationContextAccessor != null)
            {
                this.correlationContextAccessor.CorrelationContext = correlationContext;
            }

            return correlationContext;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (this.correlationContextAccessor != null)
            {
                this.correlationContextAccessor.CorrelationContext = null;
            }
        }
    }
}