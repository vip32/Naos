namespace Naos.UnitTests.Commands
{
    using System.Threading.Tasks;
    using FluentValidation;
    using FluentValidation.Results;
    using Naos.Commands.App;
    using Naos.Foundation;
    using Shouldly;
    using Xunit;

    public class ValidateCommandBehaviorTests
    {
        [Fact]
        public async Task ValidCommand_Succeeds()
        {
            // arrange/act
            var behaviorResult = new CommandBehaviorResult();
            await new ValidateCommandBehavior().ExecutePreHandleAsync(new StubCommand("Name1"), behaviorResult).AnyContext();

            // assert
            behaviorResult.ShouldNotBeNull();
            behaviorResult.Cancelled.ShouldBeFalse();
        }

        [Fact]
        public async Task InvalidatCommand_Fails()
        {
            // arrange/act
            var behaviorResult = new CommandBehaviorResult();
            var ex = await Assert.ThrowsAsync<ValidationException>(() =>
                new ValidateCommandBehavior().ExecutePreHandleAsync(new StubCommand(null), behaviorResult)).AnyContext();

            // assert
            ex.Message.ShouldContain("has validation errors");
        }

        public class StubCommand : Command<bool>
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