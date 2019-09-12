namespace Naos.Core.Commands.App
{
    using System;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Tracing.Domain;
    using Naos.Foundation;

    public class TracerCommandBehavior : ICommandBehavior, IDisposable
    {
        private readonly ILogger<TracerCommandBehavior> logger;
        private readonly ITracer tracer;
        private ICommandBehavior next;
        private IScope scope;

        /// <summary>
        /// Initializes a new instance of the <see cref="TracerCommandBehavior"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public TracerCommandBehavior(ILogger<TracerCommandBehavior> logger, ITracer tracer)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(tracer, nameof(tracer));

            this.logger = logger;
            this.tracer = tracer;
        }

        public ICommandBehavior SetNext(ICommandBehavior next)
        {
            this.next = next;
            return next;
        }

        /// <summary>
        /// Executes this behavior for the specified command.
        /// </summary>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="request">The command.</param>
        public async Task ExecutePreHandleAsync<TResponse>(Command<TResponse> request, CommandBehaviorResult result)
        {
            EnsureArg.IsNotNull(request);

            ISpan parentSpan = null;

            if (request.Properties.ContainsKey("ParentSpanId"))
            {
                parentSpan = new Span(null, request.Properties.GetValueOrDefault(CommandPropertyKeys.ParentSpanId) as string);
            }

            this.scope = this.tracer.BuildSpan(
                        $"command {request.GetType().PrettyName()}".ToLowerInvariant(),
                        LogKeys.AppCommand,
                        SpanKind.Consumer,
                        parentSpan).Activate(this.logger);

            if (!result.Cancelled && this.next != null)
            {
                await this.next.ExecutePreHandleAsync(request, result).AnyContext();
            }
            else
            {
                this.scope?.Dispose();
            }

            // terminate here
        }

        public async Task ExecutePostHandleAsync<TResponse>(CommandResponse<TResponse> response, CommandBehaviorResult result)
        {
            this.scope?.Dispose();

            if (!result.Cancelled && this.next != null)
            {
                await this.next.ExecutePostHandleAsync(response, result).AnyContext();
            }
        }

        public void Dispose()
        {
            this.scope?.Dispose();
        }
    }
}
