namespace Naos.Core.Operations.Domain
{
    using System;

    public interface IScope : IDisposable
    {
        ISpan Span { get; }
    }
}