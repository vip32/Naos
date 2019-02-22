namespace Naos.Sample.Customers.App
{
    using FluentValidation.Results;
    using Naos.Core.Commands.Domain;
    using Naos.Sample.Customers.Domain;

    public class CreateCustomerCommand : CommandRequest<string>
    {
        public CreateCustomerCommand(Customer entity)
        {
            EnsureThat.EnsureArg.IsNotNull(entity, nameof(entity));

            this.Customer = entity;
        }

        public Customer Customer { get; set; } // TODO: should be immutable

        public override ValidationResult Validate() =>
            new CreateCustomerCommandValidator().Validate(this);
    }
}
