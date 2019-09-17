namespace Naos.Sample.Customers.App
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Commands.App;
    using Naos.Core.Tracing.Domain;
    using Naos.Foundation;
    using Naos.Sample.Customers.Domain;

    public class GetActiveCustomersQueryHandler : BehaviorCommandHandler<GetActiveCustomersQuery, IEnumerable<Customer>>
    {
        private readonly ILogger<GetActiveCustomersQueryHandler> logger;
        private readonly ICustomerRepository repository;

        public GetActiveCustomersQueryHandler(
            ICustomerRepository repository,
            ILogger<GetActiveCustomersQueryHandler> logger,
            ITracer tracer = null,
            IEnumerable<ICommandBehavior> behaviors = null) // = scoped
            : base(logger, tracer, behaviors)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(repository, nameof(repository));

            this.logger = logger;
            this.repository = repository;
        }

        public override async Task<CommandResponse<IEnumerable<Customer>>> HandleRequest(GetActiveCustomersQuery request, CancellationToken cancellationToken)
        {
            return new CommandResponse<IEnumerable<Customer>>
            {
                Result = await this.repository.FindAllAsync().AnyContext()
            };
        }
    }
}
