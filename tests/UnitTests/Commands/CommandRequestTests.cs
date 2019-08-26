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
            var request = new EchoCommand
            {
                Message = "John Doe"
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
            var command = Factory.Create(typeof(EchoCommand));
            command.ShouldNotBeNull();

            // act
            var result = await mediator.Send(command).AnyContext();

            // assert
        }

        [Fact]
        public async Task Factory2Test()
        {
            // arrange
            var mediator = Substitute.For<IMediator>();
            var command = SerializationHelper.JsonDeserialize("{\"Message\": \"John Doe\"}", typeof(EchoCommand));

            // act
            var result = await mediator.Send(command).AnyContext();

            // assert
        }
    }
}