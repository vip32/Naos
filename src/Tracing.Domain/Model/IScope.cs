namespace Naos.Tracing.Domain
{
    using System;

    public interface IScope : IDisposable
    {
        ISpan Span { get; }
    }
}