namespace Naos.Foundation
{
    using System.Threading.Tasks;
    using MediatR;

    public interface IConsoleCommand
    {
        Task<bool> SendAsync(IMediator mediator);
    }
}
