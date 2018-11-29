namespace Naos.Core.Correlation.Domain
{
    using Naos.Core.Correlation.Domain.Model;

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
        /// <param name="correlationContextAccessor">The <see cref="ICorrelationContextAccessor"/> through which the <see cref="CorrelationContext"/> will be set.</param>
        public CorrelationContextFactory(ICorrelationContextAccessor correlationContextAccessor)
        {
            this.accessor = correlationContextAccessor;
        }

        /// <inheritdoc />
        public CorrelationContext Create(string correlationId, string header)
        {
            var result = new CorrelationContext(correlationId, header);

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