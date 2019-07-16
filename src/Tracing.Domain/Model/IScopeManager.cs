namespace Naos.Core.Tracing.Domain
{
    using Microsoft.Extensions.Logging;

    public interface IScopeManager
    {
        IScope Current { get; }

        IScope Activate(ISpan span, ILogger logger, bool finishOnDispose = true);

        void Deactivate(IScope scop);
    }
}