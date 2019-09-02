namespace Naos.Core.Commands.App.Web
{
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;

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

            // contiue with next extension
            await base.InvokeAsync(command, registration, context).AnyContext();
        }

        public override async Task InvokeAsync<TCommand>(
            TCommand command,
            RequestCommandRegistration<TCommand> registration,
            HttpContext context)
        {
            this.logger.LogInformation($"{{LogKey:l}} command request received (name={registration.CommandType.PrettyName()}, id={command.Id})", LogKeys.AppCommand);

            // contiue with next extension
            await base.InvokeAsync(command, registration, context).AnyContext();
        }
    }
}
