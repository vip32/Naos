namespace Naos.Core.Commands.App.Web
{
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Tracing.Domain;
    using Naos.Foundation;

    public class TracerCommandRequestExtension : CommandRequestExtension
    {
        private readonly ITracer tracer;

        public TracerCommandRequestExtension(ITracer tracer = null)
        {
            this.tracer = tracer;
        }

        public override async Task InvokeAsync<TCommand, TResponse>(
            TCommand command,
            CommandRequestRegistration<TCommand, TResponse> registration,
            HttpContext context)
        {
            if (this.tracer != null)
            {
                command.Properties.Add(CommandPropertyKeys.ParentSpanId, this.tracer.CurrentSpan.SpanId);
            }

            // TODO: or start a whole new SERVER span here, which is the parent for the COMMAND span?
            //       otherwhise the received and queued command is not visible in the trace untill it is dequeued
            //       registration.IsQueued

            // contiue with next extension
            await base.InvokeAsync(command, registration, context).AnyContext();
        }

        public override async Task InvokeAsync<TCommand>(
            TCommand command,
            CommandRequestRegistration<TCommand> registration,
            HttpContext context)
        {
            if (this.tracer != null)
            {
                command.Properties.Add(CommandPropertyKeys.ParentSpanId, this.tracer.CurrentSpan.SpanId);
            }

            // contiue with next extension
            await base.InvokeAsync(command, registration, context).AnyContext();
        }
    }
}
