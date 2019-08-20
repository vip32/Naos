namespace Naos.Core.Commands.Domain
{
    using System;
    using FluentValidation;
    using FluentValidation.Results;
    using MediatR;
    using Naos.Foundation.Domain;
    using Newtonsoft.Json;

    public class EchoCommandValidator : AbstractValidator<EchoCommand>
    {
        public EchoCommandValidator()
        {
            this.RuleFor(c => c.Message).NotNull().NotEmpty();
        }
    }
}
