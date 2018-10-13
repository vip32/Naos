namespace Naos.Core.App.Commands
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using MediatR;

    /// <summary>
    /// A base implementation for handling application commands and ensuring all behaviors are executed with proper responses (not cancelled)
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">Return value of the wrapped command handler</typeparam>
    /// <seealso cref="Commands.BaseCommandHandler{TRequest, TResponse}" />
    /// <seealso cref="MediatR.IRequestHandler{CommandRequest{TResponse}, CommandResponse{TResponse}}" />
    public abstract class BehaviorCommandHandler<TRequest, TResponse>
        : BaseCommandHandler<TRequest, TResponse>
        where TRequest : CommandRequest<TResponse>
    {
        private readonly IEnumerable<ICommandBehavior> behaviors;

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorCommandHandler{TRequest, TResponse}" /> class.
        /// </summary>
        /// <param name="mediator">The mediator.</param>
        /// <param name="behaviors">The behaviors.</param>
        protected BehaviorCommandHandler(IMediator mediator, IEnumerable<ICommandBehavior> behaviors)
            : base(mediator)
        {
            this.behaviors = behaviors;
        }

        /// <summary>
        /// Handles the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public override async Task<CommandResponse<TResponse>> Handle(TRequest request, CancellationToken cancellationToken)
        {
            foreach (var behavior in this.behaviors.NullToEmpty())
            {
                var behaviorResult = await behavior.ExecuteAsync(request).ConfigureAwait(false);
                if (behaviorResult.Cancelled) // abort if this behavior did not succeed
                {
                    // TODO: log reason
                    return new CommandResponse<TResponse>(behaviorResult.CancelledReason);
                }
            }

            return await this.HandleRequest(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
