namespace Naos.Core.Commands.App.Web
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    public interface ICommandRequestExtension
    {
        Task InvokeAsync<TCommand, TResponse>(TCommand command, CommandRequestRegistration<TCommand, TResponse> registration, HttpContext context)
            where TCommand : Command<TResponse>;

        Task InvokeAsync<TCommand>(TCommand command, RequestCommandRegistration<TCommand> registration, HttpContext context)
            where TCommand : Command<object>;

        CommandRequestBaseExtension SetNext(ICommandRequestExtension extension);
    }
}