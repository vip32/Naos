namespace Naos.Core.Tracing.Domain
{
    using System;

    public interface IScope : IDisposable
    {
        ISpan Span { get; }
    }
}