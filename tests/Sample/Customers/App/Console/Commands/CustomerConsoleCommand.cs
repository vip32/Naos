namespace Naos.Sample.Customers.App
{
    using System.Threading.Tasks;
    using CommandLine;
    using MediatR;
    using Naos.Core.Common;
    using Naos.Core.Common.Console;

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
