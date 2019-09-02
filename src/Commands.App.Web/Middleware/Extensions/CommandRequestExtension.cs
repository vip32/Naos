namespace Naos.Core.Commands.App.Web
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Naos.Foundation;

    public abstract class CommandRequestExtension : ICommandRequestExtension
    {
        private ICommandRequestExtension next;

        public CommandRequestExtension SetNext(ICommandRequestExtension extension)
        {
            this.next = extension;

            return this;
        }

        public virtual async Task InvokeAsync<TCommand, TResponse>(
            TCommand command,
            CommandRequestRegistration<TCommand, TResponse> registration,
            HttpContext context)
            where TCommand : Command<TResponse>
        {
            if (this.next != null)
            {
                await this.next.InvokeAsync<TCommand, TResponse>(command, registration, context).AnyContext();
            }

            // chain is terminated here
        }

        public virtual async Task InvokeAsync<TCommand>(
            TCommand command,
            RequestCommandRegistration<TCommand> registration,
            HttpContext context)
            where TCommand : Command<object>
        {
            if (this.next != null)
            {
                await this.next.InvokeAsync<TCommand>(command, registration, context).AnyContext();
            }

            // chain is terminated here
        }
    }
}
