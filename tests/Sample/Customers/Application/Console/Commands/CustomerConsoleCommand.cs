namespace Naos.Sample.Customers.Application
{
    using System.Threading.Tasks;
    using CommandLine;
    using MediatR;
    using Naos.Foundation;

    [Verb("customer", HelpText = "Sample customer command")]
    public class CustomerConsoleCommand : IConsoleCommand
    {
        [Option('c', "create", HelpText = "Execute a CreateCustomerCommand", Required = true)]
        public bool Create { get; set; }

        public async Task<bool> SendAsync(IMediator mediator)
        {
            return await mediator.Send(new ConsoleCommandEvent<CustomerConsoleCommand>(this)).AnyContext();
        }
    }
}
