namespace Naos.Commands.App
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Humanizer;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Tracing.Domain;

    /// <summary>
    /// A base implementation for handling application commands and ensuring all behaviors are executed with proper responses (not cancelled).
    /// </summary>
    /// <typeparam name="TCommand">The type of the request.</typeparam>
    /// <typeparam name="TResponse">Return value of the wrapped command handler.</typeparam>
    /// <seealso cref="App.CommandHandler{TRequest, TResponse}" />
    /// <seealso cref="MediatR.IRequestHandler{Command{TResponse}, CommandResponse{TResponse}}" />
    public abstract class BaseCommandHandler<TCommand, TResponse>
        : CommandHandler<TCommand, TResponse>
        where TCommand : Command<TResponse>
    {
        private readonly IEnumerable<ICommandBehavior> behaviors;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCommandHandler{TRequest, TResponse}" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="behaviors">The behaviors.</param>
        protected BaseCommandHandler(
            ILogger logger,
            ITracer tracer = null,
            IEnumerable<ICommandBehavior> behaviors = null)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.Logger = logger;
            this.Tracer = tracer; // cannot be done properly with an extra behavior (missing current scope in handlers)
            this.behaviors = behaviors;
        }

        public ILogger Logger { get; }

        public ITracer Tracer { get; }

        /// <summary>
        /// Handles the specified request. All pre/post behaviors will be called.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public override async Task<CommandResponse<TResponse>> Handle(TCommand request, CancellationToken cancellationToken)
        {
            var parentSpan = this.Tracer?.CurrentSpan;
            if (request.Properties.ContainsKey(CommandPropertyKeys.TraceId) && request.Properties.ContainsKey(CommandPropertyKeys.TraceSpanId))
            {
                // dehydrate parent span
                parentSpan = new Span(request.Properties.GetValueOrDefault(CommandPropertyKeys.TraceId) as string, request.Properties.GetValueOrDefault(CommandPropertyKeys.TraceSpanId) as string);
                parentSpan.SetSampled(request.Properties.GetValueOrDefault(CommandPropertyKeys.TraceSampled).To<bool>());
            }

            // TODO: log scope (correlationid)?
            using (var scope = this.Tracer?.BuildSpan(
                $"command {request.GetType().PrettyName()}".ToLowerInvariant(),
                LogKeys.AppCommand,
                SpanKind.Consumer,
                parentSpan).Activate(this.Logger))
            using (var timer = new Foundation.Timer())
            {
                var commandName = typeof(TCommand).PrettyName();
                this.Logger.LogJournal(LogKeys.AppCommand, $"command handle (name={commandName}, handler={this.GetType().PrettyName()}, id={request.Id})", LogPropertyKeys.TrackHandleCommand);

                var result = await this.ExecutePreHandleBehaviorsAsync(request).AnyContext();
                if (result.Cancelled) // abort if a pre behavior did not succeed
                {
                    timer.Stop();
                    this.Logger.LogInformation($"{{LogKey:l}} command cancelled (name={commandName}, handler={this.GetType().PrettyName()}, id={request.Id}) {result.CancelledReason} -> took {timer.Elapsed.Humanize()}", LogKeys.AppCommand);
                    return new CommandResponse<TResponse>(result.CancelledReason); // TODO: log reason
                }

                var response = await this.HandleRequest(request, cancellationToken).AnyContext();

                await this.ExecutePostHandleBehaviorsAsync(result, response).AnyContext();
                timer.Stop();
                if (result.Cancelled) // abort if a post behavior did not succeed
                {
                    this.Logger.LogInformation($"{{LogKey:l}} command cancelled (name={commandName}, handler={this.GetType().PrettyName()}, id={request.Id}) {result.CancelledReason} -> took {timer.Elapsed.Humanize()}", LogKeys.AppCommand);
                    return new CommandResponse<TResponse>(result.CancelledReason); // TODO: log reason
                }

                this.Logger.LogInformation($"{{LogKey:l}} command handled (name={commandName}, handler={this.GetType().PrettyName()}, id={request.Id}, cancelled={response.Cancelled}) -> took {timer.Elapsed.Humanize()}", LogKeys.AppCommand);

                return response;
            }
        }

        private async Task<CommandBehaviorResult> ExecutePreHandleBehaviorsAsync(TCommand request)
        {
            var behaviors = new List<ICommandBehavior>(this.behaviors.Safe()); // TODO: order!
            foreach (var behavior in behaviors) // build up the behaviors chain
            {
                behavior.SetNext(behaviors.NextOf(behavior));
            }

            this.Logger.LogDebug($"{{LogKey:l}} command behaviors chain: pre={behaviors.Select(e => e.GetType().PrettyName()).ToString("|")}", LogKeys.AppCommand);
            var result = new CommandBehaviorResult();
            if (behaviors.Count > 0) // execute all chained behaviors
            {
                await this.behaviors.First().ExecutePreHandleAsync(request, result).AnyContext();
            }

            return result;
        }

        private async Task ExecutePostHandleBehaviorsAsync(CommandBehaviorResult result, CommandResponse<TResponse> response)
        {
            var behaviors = new List<ICommandBehavior>(this.behaviors.Safe().Reverse()); // TODO: order!
            foreach (var behavior in behaviors) // build up the behaviors chain
            {
                behavior.SetNext(behaviors.NextOf(behavior));
            }

            this.Logger.LogDebug($"{{LogKey:l}} command behaviors chain: post={behaviors.Select(e => e.GetType().PrettyName()).ToString("|")}", LogKeys.AppCommand);
            if (behaviors.Count > 0) // execute all chained behaviors
            {
                await this.behaviors.First().ExecutePostHandleAsync(response, result).AnyContext();
            }
        }
    }
}
