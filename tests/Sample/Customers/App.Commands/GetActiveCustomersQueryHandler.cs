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

    public class GetActiveCustomersQueryHandler : BehaviorCommandHandler<GetActiveCustomersQuery, IEnumerable<Customer>>
    {
        private readonly ILogger<GetActiveCustomersQueryHandler> logger;
        private readonly ICustomerRepository repository;

        public GetActiveCustomersQueryHandler(
            ILogger<GetActiveCustomersQueryHandler> logger,
            IEnumerable<ICommandBehavior> behaviors,
            ICustomerRepository repository)
            : base(logger, behaviors)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(repository, nameof(repository));

            this.logger = logger;
            this.repository = repository;
        }

        public override async Task<CommandResponse<IEnumerable<Customer>>> HandleRequest(GetActiveCustomersQuery request, CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                return new CommandResponse<IEnumerable<Customer>>
                {
                    Result = new List<Customer> { new Customer { FirstName = "John", LastName = "Doe" } }
                };
            }).AnyContext();
        }
    }
}
