namespace Naos.Core.Commands.Domain
{
    using FluentValidation;

    public class PingCommandValidator : AbstractValidator<PingCommand>
    {
        public PingCommandValidator()
        {
            this.RuleFor(c => c.Message).NotNull().NotEmpty();
        }
    }
}
