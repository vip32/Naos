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
        private readonly ICustomerRepository customerRepository;

        public CreateCustomerCommandHandler(IMediator mediator, IEnumerable<ICommandBehavior> behaviors, ICustomerRepository customerRepository)
            : base(mediator, behaviors)
        {
            EnsureArg.IsNotNull(customerRepository);

            this.customerRepository = customerRepository;
        }

        public override async Task<CommandResponse<string>> HandleRequest(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            if(!request.Customer.Region.EqualsAny(new[] { "East", "West" }))
            {
                return new CommandResponse<string>("cannot accept customers outside regular regions");
            }

            request.Customer.SetCustomerNumber();
            var entity = await this.customerRepository.InsertAsync(request.Customer).ConfigureAwait(false);

            request.Properties.AddOrUpdate(this.GetType().Name, true);

            return new CommandResponse<string>
            {
                Result = entity.Id
            };
        }
    }
}
