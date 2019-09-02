namespace Naos.Core.Commands.App.Web
{
    using Naos.Core.Commands.App;

    public class CommandWrapper
    {
        public object Command { get; set; }

        public CommandWrapper SetCommand<TCommand, TResponse>(TCommand command)
            where TCommand : Command<TResponse>
        {
            this.Command = command;
            return this;
        }

        public CommandWrapper SetCommand<TCommand>(TCommand command)
            where TCommand : Command<object>
        {
            this.Command = command;
            return this;
        }
    }
}
