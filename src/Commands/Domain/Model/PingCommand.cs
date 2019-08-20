namespace Naos.Core.Commands.Domain
{
    using FluentValidation.Results;

    public class PingCommand : CommandRequest<object> // has no response type
    {
        public string Message { get; set; }

        public override ValidationResult Validate() =>
            new PingCommandValidator().Validate(this);
    }
}
