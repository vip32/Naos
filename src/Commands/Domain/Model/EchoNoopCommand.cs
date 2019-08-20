namespace Naos.Core.Commands.Domain
{
    using FluentValidation.Results;

    public class EchoNoopCommand : CommandRequest<object>
    {
        public string Message { get; set; }

        public override ValidationResult Validate() =>
            new EchoNoopCommandValidator().Validate(this);
    }
}
