namespace Naos.Core.UnitTests.Commands.App.Handlers.Behaviors
{
    using System.Threading.Tasks;
    using FluentValidation;
    using FluentValidation.Results;
    using Naos.Core.Commands.App;
    using Xunit;

    public class ValidateCommandBehaviorTests
    {
        [Fact]
        public async Task ValidaCommand_Succeeds()
        {
            // arrange/act
            var result = await new ValidateCommandBehavior().ExecuteAsync(new StubCommand("Name1"));

            // assert
            Assert.NotNull(result);
            Assert.False(result.Cancelled);
        }

        [Fact]
        public async Task InvalidatCommand_Fails()
        {
            // arrange/act
            var ex = await Assert.ThrowsAsync<ValidationException>(() =>
                new ValidateCommandBehavior().ExecuteAsync(new StubCommand(null))).ConfigureAwait(false);

            // assert
            Assert.Contains("has validation errors", ex.Message);
        }

        public class StubCommand : CommandRequest<bool>
        {
            public StubCommand(string name)
            {
                this.Name = name;
            }

            public string Name { get; }

            public override ValidationResult Validate() => new Validator().Validate(this);

            private class Validator : AbstractValidator<StubCommand>
            {
                public Validator()
                {
                    this.RuleFor(order => order.Name).NotEmpty().WithMessage("Name cannot be empty");
                }
            }
        }
    }
}