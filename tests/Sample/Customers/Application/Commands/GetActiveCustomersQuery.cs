namespace Naos.Sample.Customers.Application
{
    using System.Collections.Generic;
    using Naos.Commands.Application;
    using Naos.Sample.Customers.Domain;

    public class GetActiveCustomersQuery : Command<IEnumerable<Customer>>
    {
    }
}
