namespace Naos.Core.RequestCorrelation.App
{
    using System.Threading;

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