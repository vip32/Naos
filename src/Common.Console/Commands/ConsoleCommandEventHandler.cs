namespace Naos.Core.Common.Console
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;

    public abstract class ConsoleCommandEventHandler<TCommand> : IRequestHandler<ConsoleCommandEvent<TCommand>, bool>
        where TCommand : IConsoleCommand
    {
        public abstract Task<bool> Handle(ConsoleCommandEvent<TCommand> request, CancellationToken cancellationToken);
    }
}
