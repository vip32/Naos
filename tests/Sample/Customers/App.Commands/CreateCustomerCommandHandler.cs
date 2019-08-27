namespace Naos.Sample.Customers.App
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Commands.Domain;
    using Naos.Foundation;
    using Naos.Sample.Customers.Domain;

    public class CreateCustomerCommandHandler : BehaviorCommandHandler<CreateCustomerCommand, object>
    {
        private readonly ILogger<CreateCustomerCommandHandler> logger;
        private readonly ICustomerRepository repository;

        public CreateCustomerCommandHandler(
            ILogger<CreateCustomerCommandHandler> logger,
            IEnumerable<ICommandBehavior> behaviors,
            ICustomerRepository repository)
            : base(logger, behaviors)
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

            if (!request.Customer.Region.EqualsAny(new[] { "East", "West" }))
            {
                // cancels the command
                return new CommandResponse<object>("cannot accept customers outside regular regions");
            }

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
