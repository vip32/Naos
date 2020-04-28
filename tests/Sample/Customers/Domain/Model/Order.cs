namespace Naos.Sample.Customers.Domain
{
    using System;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    public class Order : AggregateRoot<string>, ITenantEntity
    {
        public string CustomerNumber { get; private set; }

        public string OrderNumber { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Region { get; set; }

        public decimal Total { get; set; }

        public string TenantId { get; set; }

        public Period DeliveryPeriod { get; private set; }

        public Period ReturnPeriod { get; private set; }

        public void SetCustomerNumber()
        {
            if (this.CustomerNumber.IsNullOrEmpty())
            {
                this.CustomerNumber = $"{RandomGenerator.GenerateString(2)}-{DateTimeOffset.UtcNow.Ticks}";
            }
        }

        public void SetNormalDelivery()
        {
            this.DeliveryPeriod = Period.Create(DateTime.UtcNow, DateTime.UtcNow.AddDays(7));
        }

        public void SetPriorityDelivery()
        {
            this.DeliveryPeriod = Period.Create(DateTime.UtcNow, DateTime.UtcNow.AddDays(3));
        }

        public void SetReturnPeriod(DateTime fromDate)
        {
            Check.Throw(this, new UnscheduledOrderDeliveryCannotBeReturnedSpecification());
            Check.Throw(this, new OrderReturnPerdiodShouldStartAfterDeliveryPerdiodStartSpecification(fromDate));

            this.ReturnPeriod = Period.Create(fromDate, fromDate.AddDays(31));
        }

        public class UnscheduledOrderDeliveryCannotBeReturnedSpecification : Specification<Order> // business rule example
        {
            public UnscheduledOrderDeliveryCannotBeReturnedSpecification()
                : base(t => t.DeliveryPeriod != null)
            {
            }

            public override string Description => "An order should be scheduled for delivery before a return period can be specified";
        }

        public class OrderReturnPerdiodShouldStartAfterDeliveryPerdiodStartSpecification : Specification<Order> // business rule example
        {
            public OrderReturnPerdiodShouldStartAfterDeliveryPerdiodStartSpecification(DateTime fromDate)
                : base(t => t.DeliveryPeriod.StartDate.HasValue && t.DeliveryPeriod.StartDate.Value <= fromDate)
            {
            }

            public override string Description => "An order should be scheduled for delivery before a return period can be specified";
        }
    }
}
