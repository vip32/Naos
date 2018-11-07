namespace Naos.Sample.Customers.App
{
    using FluentValidation;
    using FluentValidation.Results;
    using Naos.Core.App.Commands;
    using Naos.Sample.Customers.Domain;

    public class CreateCustomerCommand : CommandRequest<string>
    {
        public Customer Customer { get; set; }

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
