namespace Naos.Core.Operations.Domain
{
    public interface ISpanBuilder
    {
        ISpanBuilder AsChildOf(ISpan parent); // child

        ISpanBuilder AsReferenceFrom(ISpan parent); // reference

        ISpanBuilder IgnoreActiveSpan();

        ISpanBuilder WithTag(string key, string value);

        ISpan Build();

        IScope Activate(bool finishSpanOnDispose = true);
    }
}