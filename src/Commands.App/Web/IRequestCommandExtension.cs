namespace Naos.Core.Commands.App.Web
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    public interface IRequestCommandExtension
    {
        Task InvokeAsync<TCommand, TResponse>(TCommand command, RequestCommandRegistration<TCommand, TResponse> registration, HttpContext context)
            where TCommand : CommandRequest<TResponse>;

        Task InvokeAsync<TCommand>(TCommand command, RequestCommandRegistration<TCommand> registration, HttpContext context)
            where TCommand : CommandRequest<object>;

        RequestCommandExtension SetNext(IRequestCommandExtension extension);
    }
}