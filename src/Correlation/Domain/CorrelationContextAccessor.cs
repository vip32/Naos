namespace Naos.Core.Correlation.Domain
{
    using System.Threading;
    using Naos.Core.Correlation.Domain.Model;

    /// <inheritdoc />
    public class CorrelationContextAccessor : ICorrelationContextAccessor
    {
        private static readonly AsyncLocal<CorrelationContext> Data = new AsyncLocal<CorrelationContext>();

        /// <inheritdoc />
        public CorrelationContext Context
        {
            get => Data.Value;
            set => Data.Value = value;
        }
    }
}