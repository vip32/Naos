namespace Naos.Core.Commands.App.Web
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Tracing.Domain;
    using Naos.Foundation;

    public class TracerCommandRequestExtension : CommandRequestExtension
    {
        private readonly ILogger<TracerCommandRequestExtension> logger;
        private readonly ITracer tracer;

        public TracerCommandRequestExtension(ILogger<TracerCommandRequestExtension> logger, ITracer tracer = null)
        {
            this.logger = logger;
            this.tracer = tracer;
        }

        public override async Task InvokeAsync<TCommand, TResponse>(
            TCommand command,
            CommandRequestRegistration<TCommand, TResponse> registration,
            HttpContext context)
        {
            using (var scope = this.tracer?.BuildSpan(
                        $"command request {command.GetType().PrettyName()}".ToLowerInvariant(),
                        LogKeys.AppCommand,
                        SpanKind.Consumer).Activate(this.logger))
            {
                // start a whole new SERVER span later, which is the parent for the current 'COMMAND REQUEST' span
                command.Properties.Add(CommandPropertyKeys.TraceId, scope?.Span?.TraceId); // propagate the span infos
                command.Properties.Add(CommandPropertyKeys.ParentSpanId, scope?.Span?.SpanId); // propagate the span infos

                // continue with next extension
                await base.InvokeAsync(command, registration, context).AnyContext();
            }
        }

        public override async Task InvokeAsync<TCommand>(
            TCommand command,
            CommandRequestRegistration<TCommand> registration,
            HttpContext context)
        {
            using (var scope = this.tracer?.BuildSpan(
                        $"command request {command.GetType().PrettyName()}".ToLowerInvariant(),
                        LogKeys.AppCommand,
                        SpanKind.Consumer).Activate(this.logger))
            {
                // start a whole new SERVER span later, which is the parent for the current 'COMMAND REQUEST' span
                command.Properties.Add(CommandPropertyKeys.TraceId, scope?.Span?.TraceId); // propagate the span infos
                command.Properties.Add(CommandPropertyKeys.ParentSpanId, scope?.Span?.SpanId); // propagate the span infos

                // continue with next extension
                await base.InvokeAsync(command, registration, context).AnyContext();
            }
        }
    }
}
