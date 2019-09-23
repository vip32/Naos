namespace Naos.Commands.App
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
