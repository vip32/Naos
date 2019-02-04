namespace Naos.Sample.Customers.App
{
    using FluentValidation;
    using FluentValidation.Results;
    using Naos.Core.Commands.Domain;
    using Naos.Sample.Customers.Domain;

    public class CreateCustomerCommand : CommandRequest<string>
    {
        public CreateCustomerCommand(Customer entity)
        {
            this.Customer = entity;
        }

        public Customer Customer { get; set; } // TODO: should be immutable

        public override ValidationResult Validate() => new Validator().Validate(this);

        private class Validator : AbstractValidator<CreateCustomerCommand>
        {
            public Validator()
            {
                this.RuleFor(c => c.Customer).NotNull();
                this.RuleFor(c => c.Customer.FirstName).NotNull().NotEmpty();
                this.RuleFor(c => c.Customer.LastName).NotNull().NotEmpty();
                this.RuleFor(c => c.Customer.Email).NotNull().NotEmpty();
                this.RuleFor(c => c.Customer.CustomerNumber).Null().Empty();
            }
        }
    }
}
