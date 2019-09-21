namespace Naos.Core.Tracing.Domain
{
    using Microsoft.Extensions.Logging;

    public interface ISpanBuilder
    {
        ISpanBuilder IgnoreParentSpan();

        ISpanBuilder WithTag(string key, string value);

        ISpanBuilder SetSpanId(string spanId);

        ISpan Build();

        IScope Activate(ILogger logger, bool finishOnDispose = true);
    }
}