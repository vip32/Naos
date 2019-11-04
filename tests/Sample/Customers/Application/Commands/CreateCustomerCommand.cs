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
}
