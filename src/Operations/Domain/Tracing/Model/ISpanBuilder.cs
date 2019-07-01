namespace Naos.Core.Operations.Domain
{
    public interface ISpanBuilder
    {
        ISpanBuilder ChildOf(ISpan parent); // child

        ISpanBuilder SiblingOf(ISpan parent); // reference

        ISpanBuilder IgnoreActiveSpan();

        ISpanBuilder WithTag(string key, string value);

        ISpan Build();

        IScope Start(bool finishOnDispose = true);
    }
}