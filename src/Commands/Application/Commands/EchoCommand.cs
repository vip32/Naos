namespace Naos.Core.Commands.App
{
    using System;
    using FluentValidation.Results;
    using Naos.Core.Commands.Domain;

    public class EchoCommand : CommandRequest<EchoCommandResponse>
    {
        public string Message { get; set; }

        public double Number { get; set; }

        public DateTime DateTime { get; set; }

        public override ValidationResult Validate() =>
            new EchoCommandValidator().Validate(this);
    }
}
