namespace Naos.Sample.Customers.Application
{
    using FluentValidation.Results;
    using Naos.Commands.Application;
    using Naos.Sample.Customers.Domain;

    public class CreateCustomerCommand : Command<object>
    {
        public CreateCustomerCommand()
        {
        }

        public CreateCustomerCommand(Customer entity)
        {
            this.Customer = entity;
        }

        public Customer Customer { get; set; } // TODO: should be immutable, but settable for json deserialization

        public override ValidationResult Validate() =>
            new CreateCustomerCommandValidator().Validate(this);
    }

#pragma warning disable SA1402 // File may only contain a single type
    public class CreateCustomer2Command : Command<object>
#pragma warning restore SA1402 // File may only contain a single type
    {
        //public CreateCustomer2Command()
        //{
        //}

        //public CreateCustomerCommand(Customer entity)
        //{
        //    this.Customer = entity;
        //}

        //public Customer Customer { get; set; } // TODO: should be immutable, but settable for json deserialization

        //public override ValidationResult Validate() =>
        //    new CreateCustomerCommandValidator().Validate(this);
    }
}
