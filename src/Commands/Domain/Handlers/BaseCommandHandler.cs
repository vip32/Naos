namespace Naos.Core.Commands.Domain
{
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using MediatR;

    /// <summary>
    /// A base implementation for handling application commands
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">Return value of the wrapped command handler</typeparam>
    /// <seealso cref="MediatR.IRequestHandler{CommandRequest{TResponse}, CommandResponse{TResponse}}" />
    public abstract class BaseCommandHandler<TRequest, TResponse>
        : IRequestHandler<TRequest, CommandResponse<TResponse>>
        where TRequest : CommandRequest<TResponse>
    {
        private readonly IMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCommandHandler{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="mediator">The mediator.</param>
        protected BaseCommandHandler(IMediator mediator)
        {
            EnsureArg.IsNotNull(mediator, nameof(mediator));

            this.mediator = mediator;
        }

        /// <summary>
        /// Handles the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public virtual async Task<CommandResponse<TResponse>> Handle(TRequest request, CancellationToken cancellationToken)
        {
            return await this.HandleRequest(request, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public abstract Task<CommandResponse<TResponse>> HandleRequest(TRequest request, CancellationToken cancellationToken);

        ///// <summary>
        ///// Publishes the domain event so a domain event handler can handle it.
        ///// </summary>
        ///// <param name="domainEvent">The domain event.</param>
        ///// <returns></returns>
        //public async Task PublishDomainEvent(IDomainEvent domainEvent)
        //{
        //    EnsureArg.IsNotNull(domainEvent, nameof(domainEvent));

        //    await this.mediator.Publish(domainEvent).ConfigureAwait(false);
        //}
    }
}
