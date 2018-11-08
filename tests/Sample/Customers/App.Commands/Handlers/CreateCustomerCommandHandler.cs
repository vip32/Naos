namespace Naos.Sample.Customers.App
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Naos.Core.App.Commands;
    using Naos.Core.Common;
    using Naos.Sample.Customers.Domain;

    public class CreateCustomerCommandHandler : BehaviorCommandHandler<CreateCustomerCommand, string>
    {
        private readonly ILogger<CreateCustomerCommandHandler> logger;
        private readonly ICustomerRepository repository;

        public CreateCustomerCommandHandler(ILogger<CreateCustomerCommandHandler> logger, IMediator mediator, IEnumerable<ICommandBehavior> behaviors, ICustomerRepository repository)
            : base(mediator, behaviors)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(repository, nameof(repository));

            this.logger = logger;
            this.repository = repository;
        }

        public override async Task<CommandResponse<string>> HandleRequest(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            request.Properties.AddOrUpdate(this.GetType().Name, true);

            this.logger.LogInformation($"command: {request.GetType().Name} handled by {this.GetType().Name}");

            if(!request.Customer.Region.EqualsAny(new[] { "East", "West" }))
            {
                return new CommandResponse<string>("cannot accept customers outside regular regions");
            }

            request.Customer.SetCustomerNumber();
            request.Customer = await this.repository.InsertAsync(request.Customer).ConfigureAwait(false);

            this.logger.LogInformation($"command: {request.GetType().Name} response: {request.Customer.Id}");

            return new CommandResponse<string>
            {
                Result = request.Customer.Id
            };
        }
    }
}
