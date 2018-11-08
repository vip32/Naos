namespace Naos.Sample.Customers.App
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using MediatR;
    using Naos.Core.App.Commands;
    using Naos.Core.Common;
    using Naos.Sample.Customers.Domain;

    public class CreateCustomerCommandHandler : BehaviorCommandHandler<CreateCustomerCommand, string>
    {
        private readonly ICustomerRepository repository;

        public CreateCustomerCommandHandler(IMediator mediator, IEnumerable<ICommandBehavior> behaviors, ICustomerRepository repository)
            : base(mediator, behaviors)
        {
            EnsureArg.IsNotNull(repository);

            this.repository = repository;
        }

        public override async Task<CommandResponse<string>> HandleRequest(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            request.Properties.AddOrUpdate(this.GetType().Name, true);

            if(!request.Customer.Region.EqualsAny(new[] { "East", "West" }))
            {
                return new CommandResponse<string>("cannot accept customers outside regular regions");
            }

            request.Customer.SetCustomerNumber();
            request.Customer = await this.repository.InsertAsync(request.Customer).ConfigureAwait(false);

            return new CommandResponse<string>
            {
                Result = request.Customer.Id
            };
        }
    }
}
