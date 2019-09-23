namespace Naos.Commands.App.Web
{
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Tracing.Domain;

    public class LoggingCommandRequestExtension : CommandRequestExtension
    {
        private readonly ILogger<LoggingCommandRequestExtension> logger;

        public LoggingCommandRequestExtension(ILogger<LoggingCommandRequestExtension> logger)
        {
            EnsureArg.IsNotNull(logger);

            this.logger = logger;
        }

        public override async Task InvokeAsync<TCommand, TResponse>(
            TCommand command,
            CommandRequestRegistration<TCommand, TResponse> registration,
            HttpContext context)
        {
            this.logger.LogInformation($"{{LogKey:l}} command request received (name={registration.CommandType.PrettyName()}, id={command.Id})", LogKeys.AppCommand);

            // continue with next extension
            await base.InvokeAsync(command, registration, context).AnyContext();
        }

        public override async Task InvokeAsync<TCommand>(
            TCommand command,
            CommandRequestRegistration<TCommand> registration,
            HttpContext context)
        {
            this.logger.LogInformation($"{{LogKey:l}} command request received (name={registration.CommandType.PrettyName()}, id={command.Id})", LogKeys.AppCommand);

            // continue with next extension
            await base.InvokeAsync(command, registration, context).AnyContext();
        }
    }
}
