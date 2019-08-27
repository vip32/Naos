namespace Naos.Sample.Customers.App
{
    using System.Collections.Generic;
    using Naos.Core.Commands.Domain;
    using Naos.Sample.Customers.Domain;

    public class GetActiveCustomersQuery : CommandRequest<IEnumerable<Customer>>
    {
    }
}
