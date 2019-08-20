namespace Naos.Core.Commands.Domain
{
    using FluentValidation.Results;

    public class EchoCommand : CommandRequest<EchoCommandResponse>
    {
        public string Message { get; set; }

        public override ValidationResult Validate() =>
            new EchoCommandValidator().Validate(this);
    }
}
