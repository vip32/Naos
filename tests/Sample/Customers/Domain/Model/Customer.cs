namespace Naos.Sample.Customers.Domain
{
    using Naos.Core.Domain;

    public class Customer : Entity<string>, ITenantEntity, IAggregateRoot
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Gender { get; set; }

        public string Title { get; set; }

        public string Email { get; set; }

        public string Region { get; set; }

        public Address Address { get; set; }

        public string TenantId { get; set; }
    }
}
