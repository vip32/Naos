namespace Naos.Core.Commands.App.Web
{
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;

    public class LoggingRequestCommandExtension : RequestCommandExtension
    {
        private readonly ILogger<LoggingRequestCommandExtension> logger;

        public LoggingRequestCommandExtension(ILogger<LoggingRequestCommandExtension> logger)
        {
            EnsureArg.IsNotNull(logger);

            this.logger = logger;
        }

        public override async Task InvokeAsync<TCommand, TResponse>(
            TCommand command,
            RequestCommandRegistration<TCommand, TResponse> registration,
            HttpContext context)
        {
            this.logger.LogInformation($"{{LogKey:l}} request command received (name={registration.CommandType?.Name.SliceTill("Command").SliceTill("Query")}, id={command.Id})", LogKeys.AppCommand);

            // contiue with next extension
            await base.InvokeAsync(command, registration, context).AnyContext();
        }

        public override async Task InvokeAsync<TCommand>(
            TCommand command,
            RequestCommandRegistration<TCommand> registration,
            HttpContext context)
        {
            this.logger.LogInformation($"{{LogKey:l}} request command received (name={registration.CommandType?.Name.SliceTill("Command").SliceTill("Query")}, id={command.Id})", LogKeys.AppCommand);

            // contiue with next extension
            await base.InvokeAsync(command, registration, context).AnyContext();
        }
    }
}
