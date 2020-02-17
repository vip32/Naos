namespace Naos.Sample.IntegrationTests.Customers
{
    using Naos.Commands.Application;
    using Naos.Foundation.Domain;
    using Naos.Messaging.Domain;
    using NetArchTest.Rules;
    using Shouldly;
    using Xunit;
    using Xunit.Abstractions;

    public class ArchitectureFitnessTests
    {
        private readonly ITestOutputHelper output;
        private readonly string baseNamespace = "Naos.Sample.Customers";

        public ArchitectureFitnessTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        // TODO: move rules to policy and write report to output

        [Fact]
        public void Fitness_Messages_Should_Exist_In_Application_Only()
        {
            Types.InNamespace(this.baseNamespace)
                .That().Inherit(typeof(Message)).Or().ImplementInterface(typeof(IMessageHandler<>))
               .Should().ResideInNamespace($"{this.baseNamespace}.Application")
               .GetResult().IsSuccessful.ShouldBeTrue();
        }

        [Fact]
        public void Fitness_Message_Should_Be_Named_Correctly()
        {
            Types.InNamespace(this.baseNamespace)
               .That().Inherit(typeof(Message))
               .Should().HaveNameEndingWith("Message")
               .GetResult().IsSuccessful.ShouldBeTrue();
        }

        [Fact]
        public void Fitness_MessageHandler_Should_Be_Named_Correctly()
        {
            Types.InNamespace(this.baseNamespace)
               .That().ImplementInterface(typeof(IMessageHandler<>))
               .Should().HaveNameEndingWith("MessageHandler")
               .GetResult().IsSuccessful.ShouldBeTrue();
        }

        [Fact]
        public void Fitness_Commands_Should_Exist_In_Application_Only()
        {
            Types.InNamespace(this.baseNamespace)
               .That()/*.Inherit(typeof(Command)).Or()*/.Inherit(typeof(Command<>))
               .Should().ResideInNamespace($"{this.baseNamespace}.Application")
               .GetResult().IsSuccessful.ShouldBeTrue();
        }

        [Fact]
        public void Fitness_Commands_Should_Be_Named_Correctly()
        {
            Types.InNamespace(this.baseNamespace)
               .That()/*.Inherit(typeof(Command)).Or()*/.Inherit(typeof(Command<>))
               .Should().HaveNameEndingWith("Command").Or().HaveNameEndingWith("Query")
               .GetResult().IsSuccessful.ShouldBeTrue();
        }

        [Fact]
        public void Fitness_CommandHandlers_Should_Exist_In_Application_Only()
        {
            Types.InNamespace(this.baseNamespace)
               .That().Inherit(typeof(CommandHandler<,>))
               .Should().ResideInNamespace($"{this.baseNamespace}.Application")
               .GetResult().IsSuccessful.ShouldBeTrue();
        }

        [Fact]
        public void Fitness_CommandHandlers_Should_Be_Named_Correctly()
        {
            Types.InNamespace(this.baseNamespace)
               .That().Inherit(typeof(CommandHandler<,>))
               .Should().HaveNameEndingWith("CommandHandler").Or().HaveNameEndingWith("QueryHandler")
               .GetResult().IsSuccessful.ShouldBeTrue();
        }

        [Fact]
        public void Fitness_DomainEvents_Should_Exist_In_Domain_Only()
        {
            Types.InNamespace(this.baseNamespace)
               .That().ImplementInterface(typeof(IDomainEvent)).Or().ImplementInterface(typeof(IDomainEventHandler<>))
               //.That().HaveNameEndingWith("DomainEvent").Or().HaveNameEndingWith("DomainEventHandler")
               .Should().ResideInNamespace($"{this.baseNamespace}.Domain")
               .GetResult().IsSuccessful.ShouldBeTrue();
        }

        [Fact]
        public void Fitness_DomainEvents_Should_Be_Named_Correctly()
        {
            Types.InNamespace(this.baseNamespace)
               .That().ImplementInterface(typeof(IDomainEvent))
               .Should().HaveNameEndingWith("DomainEvent")
               .GetResult().IsSuccessful.ShouldBeTrue();
        }

        [Fact]
        public void Fitness_Specifications_Should_Exist_In_Domain_Only()
        {
            Types.InNamespace(this.baseNamespace)
               .That().ImplementInterface(typeof(ISpecification<>))
               .Should().ResideInNamespace($"{this.baseNamespace}.Domain")
               .GetResult().IsSuccessful.ShouldBeTrue();
        }

        [Fact]
        public void Fitness_Specifications_Should_Be_Named_Correctly()
        {
            Types.InNamespace(this.baseNamespace)
               .That().ImplementInterface(typeof(ISpecification<>))
               .Should().HaveNameEndingWith("Specification")
               .GetResult().IsSuccessful.ShouldBeTrue();
        }

        [Fact]
        public void Fitness_Application_Should_Not_Reference_Infrastructure_Except_In_CompositionRoot()
        {
            Types.InNamespace(this.baseNamespace)
               .That().HaveDependencyOn($"{this.baseNamespace}.Infrastructure")
               .And().ResideInNamespace($"{this.baseNamespace}.Application")
               .Should().HaveName("CompositionRoot")
               .GetResult().IsSuccessful.ShouldBeTrue();
        }

        [Fact]
        public void Fitness_Domain_Should_Not_Reference_Infrastructure()
        {
            Types.InNamespace(this.baseNamespace)
                .That().ResideInNamespace($"{this.baseNamespace}.Domain")
                .ShouldNot()
                .HaveDependencyOn($"{this.baseNamespace}.Infrastructure")
                .GetResult().IsSuccessful.ShouldBeTrue();
        }

        [Fact]
        public void Fitness_Infrastructure_Should_Not_Reference_Application()
        {
            Types.InNamespace(this.baseNamespace)
                .That().ResideInNamespace($"{this.baseNamespace}.Infrastructure")
                .ShouldNot()
                .HaveDependencyOn($"{this.baseNamespace}.Application")
                .GetResult().IsSuccessful.ShouldBeTrue();
        }

        [Fact]
        public void Fitness_Applicaiton_Should_Not_Reference_Application_Web()
        {
            Types.InNamespace(this.baseNamespace)
                .That().ResideInNamespace($"{this.baseNamespace}.Application")
                .ShouldNot()
                .HaveDependencyOn($"{this.baseNamespace}.Application.Web")
                .GetResult().IsSuccessful.ShouldBeTrue();
        }
    }
}
