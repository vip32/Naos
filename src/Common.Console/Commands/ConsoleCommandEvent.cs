namespace Naos.Core.Common.Console
{
    using MediatR;

    public class ConsoleCommandEvent<TCommand> : IRequest<bool>
        where TCommand : IConsoleCommand
    {
        public ConsoleCommandEvent(TCommand command)
        {
            this.Command = command;
        }

        public TCommand Command { get; }
    }
}
