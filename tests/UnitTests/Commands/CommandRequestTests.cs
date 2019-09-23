namespace Naos.UnitTests.Commands
{
    using System.Threading.Tasks;
    using MediatR;
    using Naos.Commands.App;
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
            var response = await mediator.Send(request).AnyContext();

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
            var response = await mediator.Send(command).AnyContext();

            // assert
        }

        [Fact]
        public async Task Factory2Test()
        {
            // arrange
            var mediator = Substitute.For<IMediator>();
            var command = SerializationHelper.JsonDeserialize("{\"Message\": \"John Doe\"}", typeof(EchoCommand));

            // act
            var response = await mediator.Send(command).AnyContext();

            // assert
        }
    }
}