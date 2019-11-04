namespace Naos.Commands.Application
{
    using FluentValidation;

    public class EchoCommandValidator : AbstractValidator<EchoCommand>
    {
        public EchoCommandValidator()
        {
            //this.RuleFor(c => c.Message).NotNull().NotEmpty();
        }
    }
}
