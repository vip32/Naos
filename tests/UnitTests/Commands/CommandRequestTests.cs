namespace Naos.Core.UnitTests.Commands
{
    using System;
    using System.Threading.Tasks;
    using FluentValidation;
    using FluentValidation.Results;
    using MediatR;
    using Naos.Core.Commands.Domain;
    using Naos.Core.Commands.Domain.Model;
    using Naos.Foundation;
    using NSubstitute;
    using Shouldly;
    using Xunit;

    public class CommandRequestTests
    {
        [Fact]
        public async Task MiscTest()
        {
            // arrange
            var mediator = Substitute.For<IMediator>();
            var request = new StubCommandRequest
            {
                FirstName = "John",
                LastName = "Doe"
            };

            // act
            var result = await mediator.Send(request).AnyContext();

            // assert
            //result.ShouldNotBeNull();
        }

        [Fact]
        public async Task Factory1Test()
        {
            // arrange
            var mediator = Substitute.For<IMediator>();
            var request = Factory<ICommandRequest<ICommandResponse>>.Create(typeof(StubCommandRequest));
            request.ShouldNotBeNull();

            // act
            var result = await mediator.Send(request).AnyContext();

            // assert
        }

        [Fact]
        public async Task Factory2Test()
        {
            // arrange
            var mediator = Substitute.For<IMediator>();
            var request = Activator.CreateInstance(typeof(StubCommandRequest), null) as ICommandRequest<ICommandResponse>;
            request.ShouldNotBeNull();

            // act
            var result = await mediator.Send(request).AnyContext();

            // assert
        }
    }

#pragma warning disable SA1402 // File may only contain a single type
    public class StubCommandRequest : BaseCommandRequest<StubCommandResponse>
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public override ValidationResult Validate() => new Validator().Validate(this);

        private class Validator : AbstractValidator<StubCommandRequest>
        {
            public Validator()
            {
                //this.RuleFor(order => order.FirstName).NotEmpty().WithMessage("FirstName cannot be empty");
                //this.RuleFor(order => order.LastName).NotEmpty().WithMessage("LastName cannot be empty");
            }
        }
    }

    public class StubCommandResponse : BaseCommandResponse
    {
        public string Message { get; set; }
    }
}