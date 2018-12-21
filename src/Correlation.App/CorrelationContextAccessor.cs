namespace Naos.Core.Correlation.App
{
    using System.Threading;
    using Naos.Core.Common.Web;

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