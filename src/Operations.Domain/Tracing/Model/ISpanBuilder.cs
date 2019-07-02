namespace Naos.Core.Operations.Domain
{
    public interface ISpanBuilder
    {
        ISpanBuilder IgnoreActiveSpan();

        ISpanBuilder WithTag(string key, string value);

        ISpan Build();

        IScope Activate(bool finishOnDispose = true);
    }
}