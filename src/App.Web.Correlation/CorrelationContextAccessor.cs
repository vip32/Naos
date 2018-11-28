namespace Naos.Core.App.Web.Correlation
{
    using System.Threading;

    /// <inheritdoc />
    public class CorrelationContextAccessor : ICorrelationContextAccessor
    {
        private static readonly AsyncLocal<CorrelationContext> LocalCorrelationContext = new AsyncLocal<CorrelationContext>();

        /// <inheritdoc />
        public CorrelationContext CorrelationContext
        {
            get => LocalCorrelationContext.Value;
            set => LocalCorrelationContext.Value = value;
        }
    }
}