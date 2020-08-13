namespace Naos.Sample.Customers.Application
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Commands.Application;
    using Naos.Foundation;
    using Naos.Sample.Customers.Domain;
    using Naos.Tracing.Domain;

    public class GetActiveCustomersQueryHandler : BaseCommandHandler<GetActiveCustomersQuery, IEnumerable<Customer>>
    {
        private readonly ILogger<GetActiveCustomersQueryHandler> logger;
        private readonly ICustomerRepository repository;
        private readonly IHttpClientFactory httpClientFactory;

        public GetActiveCustomersQueryHandler(
            ICustomerRepository repository,
            IHttpClientFactory httpClientFactory,
            ILogger<GetActiveCustomersQueryHandler> logger,
            ITracer tracer = null,
            IEnumerable<ICommandBehavior> behaviors = null) // = scoped
            : base(logger, tracer, behaviors)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(repository, nameof(repository));
            EnsureArg.IsNotNull(httpClientFactory, nameof(httpClientFactory));

            this.logger = logger;
            this.repository = repository;
            this.httpClientFactory = httpClientFactory;
        }

        public override async Task<CommandResponse<IEnumerable<Customer>>> HandleRequest(GetActiveCustomersQuery request, CancellationToken cancellationToken)
        {
            using (var scope = this.Tracer?.BuildSpan("http get /api/customers DUMMY1").Activate(this.logger))
            {
                // do nothing
            }

            using (var scope = this.Tracer?.BuildSpan("dummy2 dummy2 dummy2").Activate(this.logger))
            {
                // do nothing
            }

#pragma warning disable IDE0067 // Dispose objects before losing scope
#pragma warning disable CA2000 // Dispose objects before losing scope
            var client = this.httpClientFactory.CreateClient("default");
            await client.GetAsync("https://enjkefte1b0qq.x.pipedream.net/", cancellationToken).AnyContext();

            return new CommandResponse<IEnumerable<Customer>>
            {
                Result = await this.repository.FindAllAsync(cancellationToken: cancellationToken).AnyContext()
            };
        }
    }
}
