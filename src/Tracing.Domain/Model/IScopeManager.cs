namespace Naos.Core.Tracing.Domain
{
    public interface IScopeManager
    {
        IScope Current { get; }

        IScope Activate(ISpan span, bool finishOnDispose = true);

        void Deactivate(IScope scop);
    }
}