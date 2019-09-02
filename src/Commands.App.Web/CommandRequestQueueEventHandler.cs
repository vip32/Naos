namespace Naos.Core.Commands.App.Web
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Queueing.Domain;
    using Naos.Foundation;
    using Newtonsoft.Json.Linq;

    public class CommandRequestQueueEventHandler : QueueEventHandler<CommandRequestWrapper>
    {
        private readonly ILogger<CommandRequestQueueEventHandler> logger;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly CommandRequestStorage storage;

        public CommandRequestQueueEventHandler(
            ILogger<CommandRequestQueueEventHandler> logger,
            IServiceScopeFactory serviceScopeFactory,
            CommandRequestStorage storage = null)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(serviceScopeFactory, nameof(serviceScopeFactory));

            this.logger = logger;
            this.serviceScopeFactory = serviceScopeFactory;
            this.storage = storage;
        }

        public override async Task<bool> Handle(QueueEvent<CommandRequestWrapper> request, CancellationToken cancellationToken)
        {
            if (request?.Item?.Data?.Command != null)
            {
                // as this handler is called inside a singleton scope (due to CommandRequestQueueProcessor:ProcessItems)
                // any ctor injections are also singleton. We need scoped commandhandlers (mediator default), so
                // that is achieved by creating a scope explicity
                using (var scope = this.serviceScopeFactory.CreateScope())
                {
                    this.logger.LogInformation($"{{LogKey:l}} command request dequeued (name={request.Item.Data.Command.GetType().PrettyName()}, id={request.Item.Data.Command?.Id}, type=queue)", LogKeys.AppCommand);

                    // TODO: start command TRACER
                    var mediator = scope.ServiceProvider.GetService<IMediator>(); // =scoped
                    try
                    {
                        request.Item.Data.Started = DateTimeOffset.UtcNow;
                        var response = await mediator.Send(request.Item.Data.Command).AnyContext(); // handler will be scoped too
                        request.Item.Data.Completed = DateTimeOffset.UtcNow;
                        request.Item.Data.Status = CommandRequestStates.Finished;

                        if (response != null)
                        {
                            var jResponse = JObject.FromObject(response);
                            if (!jResponse.GetValueByPath<bool>("cancelled"))
                            {
                                var resultToken = jResponse.SelectToken("result") ?? jResponse.SelectToken("Result");
                                request.Item.Data.Response = resultToken?.ToObject<object>();
                            }
                            else
                            {
                                request.Item.Data.Status = CommandRequestStates.Cancelled;
                                request.Item.Data.StatusDescription = jResponse.GetValueByPath<string>("cancelledReason");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        request.Item.Data.Status = CommandRequestStates.Failed;
                        request.Item.Data.StatusDescription = ex.GetFullMessage();

                        this.logger.LogCritical(ex, ex.Message);
                    }

                    // OPTIONAL: store request.Item.Data somewhere (repo/filestorage), then for async commands the request.Item.Data.Response can be retrieved by a client
                    if (this.storage != null)
                    {
                        this.logger.LogInformation($"SAVE {request.Item.Data.Id}");
                        await this.storage.SaveAsync(request.Item.Data).AnyContext();
                    }
                }
            }

            return true;
        }
    }
}
