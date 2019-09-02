namespace Naos.Core.Commands.App.Web
{
    using System.Threading.Tasks;
    using EnsureThat;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Newtonsoft.Json.Linq;

    public class MediatorDispatcherCommandRequestExtension : CommandRequestExtension
    {
        private readonly ILogger<LoggingCommandRequestExtension> logger;
        private readonly IMediator mediator;

        public MediatorDispatcherCommandRequestExtension(
            ILogger<LoggingCommandRequestExtension> logger,
            IMediator mediator)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(mediator, nameof(mediator));

            this.logger = logger;
            this.mediator = mediator;
        }

        public override async Task InvokeAsync<TCommand, TResponse>(
            TCommand command,
            CommandRequestRegistration<TCommand, TResponse> registration,
            HttpContext context)
        {
            this.logger.LogInformation($"{{LogKey:l}} request command dispatch (name={registration.CommandType.PrettyName()}, id={command.Id}), type=mediator)", LogKeys.AppCommand);

            // TODO: start command TRACER
            var response = await this.mediator.Send(command).AnyContext(); // https://github.com/jbogard/MediatR/issues/385
            if (response != null)
            {
                var jResponse = JObject.FromObject(response);
                if (!jResponse.GetValueByPath<bool>("cancelled"))
                {
                    var resultToken = jResponse.SelectToken("result") ?? jResponse.SelectToken("Result");
                    registration?.OnSuccess?.Invoke(command, context);

                    if (!resultToken.IsNullOrEmpty())
                    {
                        context.Response.WriteJson(resultToken);
                    }
                }
                else
                {
                    var cancelledReason = jResponse.GetValueByPath<string>("cancelledReason");
                    await context.Response.BadRequest(cancelledReason.SliceTill(":")).AnyContext();
                }
            }

            await context.Response.Header("x-commandid", command.Id).AnyContext();
            // the extension chain is terminated here
        }

        public override async Task InvokeAsync<TCommand>(
            TCommand command,
            RequestCommandRegistration<TCommand> registration,
            HttpContext context)
        {
            this.logger.LogInformation($"{{LogKey:l}} request command dispatch (name={registration.CommandType.PrettyName()}, id={command.Id}), type=mediator)", LogKeys.AppCommand);

            var response = await this.mediator.Send(command).AnyContext(); // https://github.com/jbogard/MediatR/issues/385
            if (response != null)
            {
                var jResponse = JObject.FromObject(response);
                if (!jResponse.GetValueByPath<bool>("cancelled"))
                {
                    registration?.OnSuccess?.Invoke(command, context);
                }
                else
                {
                    var cancelledReason = jResponse.GetValueByPath<string>("cancelledReason");
                    await context.Response.BadRequest(cancelledReason.SliceTill(":")).AnyContext();
                }
            }

            await context.Response.Header("x-commandid", command.Id).AnyContext();
            // the extension chain is terminated here
        }
    }
}
