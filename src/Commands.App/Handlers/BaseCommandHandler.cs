namespace Naos.Core.Commands.App
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Naos.Foundation;

    /// <summary>
    /// A base implementation for handling application commands.
    /// </summary>
    /// <typeparam name="TCommand">The type of the request.</typeparam>
    /// <typeparam name="TResponse">Return value of the wrapped command handler.</typeparam>
    /// <seealso cref="MediatR.IRequestHandler{Command{TResponse}, CommandResponse{TResponse}}" />
    public abstract class BaseCommandHandler<TCommand, TResponse>
        : IRequestHandler<TCommand, CommandResponse<TResponse>>
        where TCommand : Command<TResponse>
    {
        /// <summary>
        /// Handles the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public virtual async Task<CommandResponse<TResponse>> Handle(TCommand request, CancellationToken cancellationToken)
        {
            return await this.HandleRequest(request, cancellationToken).AnyContext();
        }

        /// <summary>
        /// Handles the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public abstract Task<CommandResponse<TResponse>> HandleRequest(TCommand request, CancellationToken cancellationToken);
    }
}
