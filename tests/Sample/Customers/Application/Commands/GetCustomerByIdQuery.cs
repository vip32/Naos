namespace Naos.Sample.Customers.Application
{
    using Naos.Commands.Application;
    using Naos.Sample.Customers.Domain;

    public class GetCustomerByIdQuery : Command<Customer>
    {
        public string CustomerId { get; set; }
    }
}
