namespace Naos.Commands.Application.Web
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Tracing.Domain;

    public class TracerCommandRequestExtension : CommandRequestExtension
    {
        private readonly ILogger<TracerCommandRequestExtension> logger;
        private readonly ITracer tracer;

        public TracerCommandRequestExtension(
            ILogger<TracerCommandRequestExtension> logger,
            ITracer tracer = null)
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
                command.Properties.Add(CommandPropertyKeys.TraceSpanId, scope?.Span?.SpanId); // propagate the span infos
                command.Properties.Add(CommandPropertyKeys.TraceSampled, scope?.Span?.IsSampled); // propagate the span infos

                // continue with next extension
                await base.InvokeAsync(command, registration, context).AnyContext();
            }
        }

        public override async Task InvokeAsync<TCommand>(
            TCommand command,
            CommandRequestRegistration<TCommand> registration,
            HttpContext context)
        {
            if (this.tracer == null)
            {
                // continue with next extension
                await base.InvokeAsync(command, registration, context).AnyContext();
            }
            else
            {
                using (var scope = this.tracer.BuildSpan(
                            $"command request {command.GetType().PrettyName()}".ToLowerInvariant(),
                            LogKeys.AppCommand,
                            SpanKind.Consumer).Activate(this.logger))
                {
                    // start a whole new SERVER span later, which is the parent for the current 'COMMAND REQUEST' span
                    command.Properties.Add(CommandPropertyKeys.TraceId, scope.Span.TraceId); // propagate the span infos
                    command.Properties.Add(CommandPropertyKeys.TraceSpanId, scope.Span.SpanId); // propagate the span infos
                    command.Properties.Add(CommandPropertyKeys.TraceSampled, scope.Span.IsSampled); // propagate the span infos

                    if (scope.Span.IsSampled == false)
                    {
                        this.logger.LogDebug($"{{LogKey:l}} span not sampled (id={scope.Span.SpanId})", LogKeys.Tracing);
                    }
                    else
                    {
                        this.logger.LogDebug($"{{LogKey:l}} span sampled (id={scope.Span.SpanId})", LogKeys.Tracing);
                    }

                    // continue with next extension
                    await base.InvokeAsync(command, registration, context).AnyContext();
                }
            }
        }
    }
}
