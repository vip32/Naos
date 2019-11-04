namespace Naos.Sample.Customers.Application
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Commands.Application;
    using Naos.Foundation;
    using Naos.Sample.Customers.Domain;
    using Naos.Tracing.Domain;

    public class CreateCustomerCommandHandler : BaseCommandHandler<CreateCustomerCommand, object>
    {
        private readonly ILogger<CreateCustomerCommandHandler> logger;
        private readonly ICustomerRepository repository;

        public CreateCustomerCommandHandler(
            ICustomerRepository repository,
            ILogger<CreateCustomerCommandHandler> logger,
            ITracer tracer = null,
            IEnumerable<ICommandBehavior> behaviors = null)
            : base(logger, tracer, behaviors)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(repository, nameof(repository));

            this.logger = logger;
            this.repository = repository;
        }

        public override async Task<CommandResponse<object>> HandleRequest(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            //this.Logger.LogJournal(LogEventPropertyKeys.TrackHandleCommand, $"{{LogKey:l}} handle {request.GetType().Name.SubstringTill("Command")}", args: LogEventKeys.AppCommand);

            request.Properties.AddOrUpdate(this.GetType().Name, true);

            this.logger.LogInformation($"{{LogKey:l}} {request.GetType().Name} (handler={this.GetType().Name})", LogKeys.AppCommand);

            request.Customer.SetCustomerNumber();
            request.Customer = await this.repository.InsertAsync(request.Customer).AnyContext();

            this.logger.LogInformation($"{{LogKey:l}} {request.GetType().Name} (response={request.Customer.Id})", LogKeys.AppCommand);

            // TODO: publish CreatedCustomer message (messaging)

            return new CommandResponse<object>
            {
                //Result = request.Customer.Id
            };
        }
    }
}
