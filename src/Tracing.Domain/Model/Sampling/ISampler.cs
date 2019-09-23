namespace Naos.Tracing.Domain
{
    using System;
    using EnsureThat;
    using Naos.Foundation;

    public interface ISampler
    {
        /// <summary>
        /// Sets whether or not the trace/span should be sampled.
        /// </summary>
        /// <param name="span">The span.</param>
        void SetSampled(ISpan span);
    }
}