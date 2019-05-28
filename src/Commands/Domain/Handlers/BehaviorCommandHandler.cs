namespace Naos.Core.Commands.Domain
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;

    /// <summary>
    /// A base implementation for handling application commands and ensuring all behaviors are executed with proper responses (not cancelled).
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">Return value of the wrapped command handler.</typeparam>
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
        /// <param name="behaviors">The behaviors.</param>
        protected BehaviorCommandHandler(
            ILogger logger,
            IEnumerable<ICommandBehavior> behaviors)
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
        public override async Task<CommandResponse<TResponse>> Handle(TRequest request, CancellationToken cancellationToken)
        {
            foreach(var behavior in this.behaviors.Safe())
            {
                var behaviorResult = await behavior.ExecuteAsync(request).AnyContext();
                if(behaviorResult.Cancelled) // abort if this behavior did not succeed
                {
                    // TODO: log reason
                    return new CommandResponse<TResponse>(behaviorResult.CancelledReason);
                }
            }

            var commandName = typeof(TRequest).Name.SliceTill("Command");
            this.Logger.LogJournal(LogKeys.AppCommand, $"handle (name={commandName}, id={request.Id})", LogEventPropertyKeys.TrackHandleCommand);
            this.Logger.LogTraceEvent(LogKeys.AppCommand, request.Id, commandName, LogTraceEventNames.Command);

            using(var timer = new Common.Timer())
            {
                var result = await this.HandleRequest(request, cancellationToken).AnyContext();

                timer.Stop();
                this.Logger.LogTraceEvent(LogKeys.AppCommand, request.Id, commandName, LogTraceEventNames.Command, timer.Elapsed);
                return result;
            }
        }
    }
}
