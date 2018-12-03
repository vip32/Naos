namespace Naos.Core.App.Correlation
{
    /// <summary>
    /// Provides access to the <see cref="CorrelationContext"/> for the current request.
    /// </summary>
    public interface ICorrelationContextAccessor
    {
        /// <summary>
        /// The <see cref="CorrelationContext"/> for the current request.
        /// </summary>
        CorrelationContext Context { get; set; }
    }
}
