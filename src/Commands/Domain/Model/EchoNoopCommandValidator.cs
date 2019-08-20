namespace Naos.Core.Commands.Domain
{
    using FluentValidation;

    public class EchoNoopCommandValidator : AbstractValidator<EchoNoopCommand>
    {
        public EchoNoopCommandValidator()
        {
            this.RuleFor(c => c.Message).NotNull().NotEmpty();
        }
    }
}
