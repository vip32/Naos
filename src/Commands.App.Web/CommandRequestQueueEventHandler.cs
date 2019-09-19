namespace Naos.Core.Commands.App.Web
{
    using System;
    using System.Collections.Generic;
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
        private readonly CommandRequestStore storage;

        public CommandRequestQueueEventHandler(
            ILogger<CommandRequestQueueEventHandler> logger,
            IServiceScopeFactory serviceScopeFactory,
            CommandRequestStore storage = null)
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
                using (this.logger.BeginScope(new Dictionary<string, object>
                {
                    [LogPropertyKeys.CorrelationId] = request.Item.Data.CorrelationId
                }))
                {
                    // as this handler is called inside a singleton scope (due to CommandRequestQueueProcessor:ProcessItems)
                    // any ctor injections are also singleton. We need scoped commandhandlers (mediator default), so
                    // that is achieved by creating a scope explicity
                    using (var scope = this.serviceScopeFactory.CreateScope())
                    {
                        this.logger.LogInformation($"{{LogKey:l}} command request dequeued (name={request.Item.Data.Command.GetType().PrettyName()}, id={request.Item.Data.Command?.Id}, type=queue)", LogKeys.AppCommand);

                        var mediator = scope.ServiceProvider.GetService<IMediator>(); // =scoped
                        try
                        {
                            request.Item.Data.Started = DateTimeOffset.UtcNow;
                            await this.StoreCommand(request).AnyContext();

                            // TODO: use for new tracer scope request.Item.Data.ParentSpanId
                            var response = await mediator.Send(request.Item.Data.Command).AnyContext(); // handler will be scoped too

                            request.Item.Data.Completed = DateTimeOffset.UtcNow;
                            request.Item.Data.Status = CommandRequestStatus.Finished;

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
                                    request.Item.Data.Status = CommandRequestStatus.Cancelled;
                                    request.Item.Data.StatusDescription = jResponse.GetValueByPath<string>("cancelledReason");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            request.Item.Data.Status = CommandRequestStatus.Failed;
                            request.Item.Data.StatusDescription = ex.GetFullMessage();

                            this.logger.LogCritical(ex, ex.Message);
                        }

                        await this.StoreCommand(request).AnyContext();
                    }
                }
            }

            return true;
        }

        private async Task StoreCommand(QueueEvent<CommandRequestWrapper> request)
        {
            if (this.storage != null)
            {
                // optionaly store the command/response so it can later be retrieved by the client (because the command was queued with no direct response)
                await this.storage.SaveAsync(request.Item.Data).AnyContext();
            }
        }
    }
}
