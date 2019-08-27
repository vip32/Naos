namespace Naos.Sample.Customers.App
{
    using System.Collections.Generic;
    using Naos.Sample.Customers.Domain;

    public class GetActiveCustomersQueryResponse
    {
        public IEnumerable<Customer> Data { get; set; }
    }
}
