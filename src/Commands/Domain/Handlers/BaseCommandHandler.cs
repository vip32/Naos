namespace Naos.Core.Commands.Domain
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Naos.Foundation;

    /// <summary>
    /// A base implementation for handling application commands.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">Return value of the wrapped command handler.</typeparam>
    /// <seealso cref="MediatR.IRequestHandler{CommandRequest{TResponse}, CommandResponse{TResponse}}" />
    public abstract class BaseCommandHandler<TRequest, TResponse>
        : IRequestHandler<TRequest, CommandResponse<TResponse>>
        where TRequest : CommandRequest<TResponse>
    {
        /// <summary>
        /// Handles the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public virtual async Task<CommandResponse<TResponse>> Handle(TRequest request, CancellationToken cancellationToken)
        {
            return await this.HandleRequest(request, cancellationToken).AnyContext();
        }

        /// <summary>
        /// Handles the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public abstract Task<CommandResponse<TResponse>> HandleRequest(TRequest request, CancellationToken cancellationToken);
    }
}
