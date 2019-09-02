namespace Naos.Core.Commands.App.Web
{
    using Naos.Core.Commands.App;

    public class CommandWrapper
    {
        public string Status { get; set; } = CommandRequestStates.Accepted;

        public string StatusDescription { get; set; } // Finished datetime/ Failure message

        public object Command { get; set; }

        public object Response { get; set; }

        //public CommandWrapper SetCommand<TCommand, TResponse>(TCommand command)
        //    where TCommand : Command<TResponse>
        //{
        //    this.Command = command;
        //    return this;
        //}

        public CommandWrapper SetCommand(object command)
        {
            this.Command = command;
            return this;
        }

        //public CommandWrapper SetCommand<TCommand>(TCommand command)
        //    where TCommand : Command<object>
        //{
        //    this.Command = command;
        //    return this;
        //}
    }
}
