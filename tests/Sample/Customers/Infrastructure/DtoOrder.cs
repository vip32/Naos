namespace Naos.Sample.Customers.Infrastructure
{
    using System;
    using Naos.Foundation.Domain;
    using Naos.Foundation.Infrastructure;

    public class DtoOrder : ICosmosEntity
    {
        public string Id { get; set; }

        public string Customer { get; set; }

        public string Order { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Location { get; set; }

        public decimal Total { get; set; }

        public string TenantId { get; set; }

        public DateTime? DeliveryStartDate { get; set; }

        public DateTime? DeliveryEndDate { get; set; }

        public DateTime? ReturnStartDate { get; set; }

        public DateTime? ReturnEndDate { get; set; }

        public State State { get; set; } = new State();
    }
}
