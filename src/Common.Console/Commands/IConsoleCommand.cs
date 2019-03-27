namespace Naos.Core.Common.Console
{
    using System.Threading.Tasks;
    using MediatR;

    public interface IConsoleCommand
    {
        Task<bool> SendAsync(IMediator mediator);
    }
}
