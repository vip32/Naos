namespace Naos.Sample.Customers.App
{
    using FluentValidation;
    using Naos.Foundation;

    public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
    {
        public CreateCustomerCommandValidator()
        {
            this.RuleFor(c => c.Customer).NotNull();
            this.RuleFor(c => c.Customer.FirstName).NotNull().NotEmpty();
            this.RuleFor(c => c.Customer.LastName).NotNull().NotEmpty();
            this.RuleFor(c => c.Customer.Email).NotNull().NotEmpty();
            this.RuleFor(c => c.Customer.Region).NotNull().NotEmpty()
                .Must(c => c.EqualsAny(new[] { "East", "West" })).WithMessage("should equal \"East\" or \"West\"");
            this.RuleFor(c => c.Customer.CustomerNumber).Null().Empty();
        }
    }
}
