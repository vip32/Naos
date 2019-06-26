namespace Naos.Sample.Customers.App
{
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using MediatR;
    using Naos.Foundation;

    public class CustomerConsoleCommandEventHandler : ConsoleCommandEventHandler<CustomerConsoleCommand>
    {
        private readonly IMediator mediator;

        public CustomerConsoleCommandEventHandler(IMediator mediator)
        {
            EnsureArg.IsNotNull(mediator, nameof(mediator));

            this.mediator = mediator;
        }

        public override async Task<bool> Handle(ConsoleCommandEvent<CustomerConsoleCommand> request, CancellationToken cancellationToken)
        {
            if(request.Command.Create)
            {
                var command = new CreateCustomerCommand(
                    new Domain.Customer
                    {
                        Id = IdGenerator.Instance.Next,
                        FirstName = "John",
                        LastName = RandomGenerator.GenerateString(4, false),
                        Email = $"John.{RandomGenerator.GenerateString(5, lowerCase: true)}@gmail.com"
                    });

                var response = await this.mediator.Send(command).AnyContext();
            }

            return true;
        }
    }
}
