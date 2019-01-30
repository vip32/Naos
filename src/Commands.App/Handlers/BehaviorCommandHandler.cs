namespace Naos.Core.Commands.App
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using EnsureThat;
    using MediatR;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A base implementation for handling application commands and ensuring all behaviors are executed with proper responses (not cancelled)
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">Return value of the wrapped command handler</typeparam>
    /// <seealso cref="App.BaseCommandHandler{TRequest, TResponse}" />
    /// <seealso cref="MediatR.IRequestHandler{CommandRequest{TResponse}, CommandResponse{TResponse}}" />
    public abstract class BehaviorCommandHandler<TRequest, TResponse>
        : BaseCommandHandler<TRequest, TResponse>
        where TRequest : CommandRequest<TResponse>
    {
        private readonly IEnumerable<ICommandBehavior> behaviors;

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorCommandHandler{TRequest, TResponse}" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="mediator">The mediator.</param>
        /// <param name="behaviors">The behaviors.</param>
        protected BehaviorCommandHandler(ILogger logger, IMediator mediator, IEnumerable<ICommandBehavior> behaviors)
            : base(mediator)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.Logger = logger;
            this.behaviors = behaviors;
        }

        public ILogger Logger { get; }

        /// <summary>
        /// Handles the specified request. All behaviors will be called first.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public override async Task<CommandResponse<TResponse>> Handle(TRequest request, CancellationToken cancellationToken)
        {
            foreach (var behavior in this.behaviors.Safe())
            {
                var behaviorResult = await behavior.ExecuteAsync(request).ConfigureAwait(false);
                if (behaviorResult.Cancelled) // abort if this behavior did not succeed
                {
                    // TODO: log reason
                    return new CommandResponse<TResponse>(behaviorResult.CancelledReason);
                }
            }

            this.Logger.LogJournal(LogEventPropertyKeys.TrackHandleCommand, $"{{LogKey:l}} handle {typeof(TRequest).Name.SubstringTill("Command")}", args: LogEventKeys.AppCommand);

            return await this.HandleRequest(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
