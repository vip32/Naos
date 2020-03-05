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

    public class GetCustomerByIdQueryHandler : BaseCommandHandler<GetCustomerByIdQuery, Customer>
    {
        private readonly ILogger<GetActiveCustomersQueryHandler> logger;
        private readonly ICustomerRepository repository;

        public GetCustomerByIdQueryHandler(
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

        public override async Task<CommandResponse<Customer>> HandleRequest(GetCustomerByIdQuery request, CancellationToken cancellationToken)
        {
            return new CommandResponse<Customer>
            {
                Result = await this.repository.FindOneAsync(request.CustomerId).AnyContext()
            };
        }
    }
}
