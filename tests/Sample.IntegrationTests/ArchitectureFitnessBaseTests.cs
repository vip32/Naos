namespace Naos.Sample.IntegrationTests
{
    using Microsoft.Extensions.Logging;
    using Naos.Commands.Application;
    using Naos.Foundation.Domain;
    using Naos.Foundation.Utilities.Xunit;
    using Naos.Messaging.Domain;
    using NetArchTest.Rules;
    using NetArchTest.Rules.Policies;
    using Shouldly;
    using Xunit;
    using Xunit.Abstractions;

    public abstract class ArchitectureFitnessBaseTests
    {
        private readonly ITestOutputHelper output;
        private readonly string baseNamespace;

        protected ArchitectureFitnessBaseTests(ITestOutputHelper output, string baseNamespace)
        {
            this.output = output;
            this.baseNamespace = baseNamespace;
        }

        [Fact]
        [FitnessTest]
        [Trait("Category", "Fitness")]
        protected virtual void ArchitectureFitnessPolicyTest()
        {
            var policy = Policy.Define("Architecture Fitness Policy", "This policy contains a valid passing policy based on Domain Driven Design guidelines")
               .For(Types.InNamespace(this.baseNamespace))
               // application messaging rules
               .Add(t => t.That()
                   .Inherit(typeof(Message)).Or().ImplementInterface(typeof(IMessageHandler<>))
                   .Should().ResideInNamespace($"{this.baseNamespace}.Application"),
                   $"{LogKeys.AppMessaging}:Layering", "Messages should only exist in the Application layer")
               .Add(t => t.That()
                   .Inherit(typeof(Message))
                   .Should().HaveNameEndingWith("Message"),
                   $"{LogKeys.AppMessaging}:Naming", "Messages should be named correctly")
               .Add(t => t.That()
                   .ImplementInterface(typeof(IMessageHandler<>))
                   .Should().HaveNameEndingWith("MessageHandler"),
                   $"{LogKeys.AppMessaging}:Naming", "MessageHandlers should be named correctly")
               // application command rules
               .Add(t => t.That()
                   /*.Inherit(typeof(Command)).Or()*/.Inherit(typeof(Command<>))
               .Should().ResideInNamespace($"{this.baseNamespace}.Application"),
                   $"{LogKeys.AppCommand}:Layering", "Commands should only exist in the Application layer")
               .Add(t => t.That()
                   /*.Inherit(typeof(Command)).Or()*/.Inherit(typeof(Command<>))
               .Should().HaveNameEndingWith("Command").Or().HaveNameEndingWith("Query"),
                   $"{LogKeys.AppCommand}:Naming", "Commands or Queries should be named correctly")
               .Add(t => t.That()
                   .Inherit(typeof(CommandHandler<,>))
                   .Should().ResideInNamespace($"{this.baseNamespace}.Application"),
                   $"{LogKeys.AppCommand}:Layering", "CommandHandlers should only exist in the Application layer")
               .Add(t => t.That()
                   .Inherit(typeof(CommandHandler<,>))
                   .Should().HaveNameEndingWith("CommandHandler").Or().HaveNameEndingWith("QueryHandler"),
                   $"{LogKeys.AppCommand}:Layering", "CommandHandlers should only exist in the Application layer")
               // domain event rules
               .Add(t => t.That()
                   .ImplementInterface(typeof(IDomainEvent))
                   .Should().ResideInNamespace($"{this.baseNamespace}.Domain"),
                   $"{LogKeys.DomainEvent}:Layering", "DomainEvents should only exist in the Domain layer")
               .Add(t => t.That()
                   .ImplementInterface(typeof(IDomainEventHandler<>))
                   .Should().ResideInNamespace($"{this.baseNamespace}.Domain"),
                   $"{LogKeys.DomainEvent}:Layering", "DomainEventHandlers should only exist in the Domain layer")
               .Add(t => t.That()
                   .ImplementInterface(typeof(IDomainEvent))
                   .Should().HaveNameEndingWith("DomainEvent"),
                   $"{LogKeys.DomainEvent}:Naming", "DomainEvents should be named correctly")
               .Add(t => t.That()
                   .ImplementInterface(typeof(IDomainEventHandler<>))
                   .Should().HaveNameEndingWith("DomainEventHandler"),
                   $"{LogKeys.DomainEvent}:Naming", "DomainEventHandlers should be named correctly")
               // domain repository rules
               .Add(t => t.That()
                    .ImplementInterface(typeof(IGenericRepository<>))
                    .Should().ResideInNamespace($"{this.baseNamespace}.Domain").Or().ResideInNamespace($"{this.baseNamespace}.Infrastructure"),
                    $"{LogKeys.DomainRepository}:Layering", "Classes that implement IGenericRepository must reside in the Domain or Infrastructure namespace")
               .Add(t => t.That()
                    .ImplementInterface(typeof(IReadOnlyGenericRepository<>))
                    .Should().ResideInNamespace($"{this.baseNamespace}.Domain").Or().ResideInNamespace($"{this.baseNamespace}.Infrastructure"),
                    $"{LogKeys.DomainRepository}:Layering", "Classes that implement IReadOnlyGenericRepository must reside in the Domain or Infrastructure namespace")
               .Add(t => t.That()
                    .ImplementInterface(typeof(IGenericRepository<>)).Or().ImplementInterface(typeof(IReadOnlyGenericRepository<>))
                    .Should().HaveNameEndingWith("Repository"),
                    $"{LogKeys.DomainRepository}:Naming", "Repositories should be named correctly")
               // domain specification rules
               .Add(t => t.That()
                   .ImplementInterface(typeof(ISpecification<>))
                   .Should().ResideInNamespace($"{this.baseNamespace}.Domain"),
                   $"{LogKeys.DomainSpecification}:Layering", "Specifications should only exist in the Domain layer")
               .Add(t => t.That()
                   .ImplementInterface(typeof(ISpecification<>))
                   .Should().HaveNameEndingWith("Specification"),
                   $"{LogKeys.DomainSpecification}:Naming", "Specifications should be named correctly")
               // general naming rules
               .Add(t => t.That()
                   .AreClasses().Or().AreInterfaces()
                   .ShouldNot().HaveNameEndingWith("Helper"),
                   $"{LogKeys.DomainSpecification}:Naming", "Classes or Interfaces named 'Helper' are not allowed")
               .Add(t => t.That()
                   .AreInterfaces()
                   .Should().HaveNameStartingWith("I"),
                   $"{LogKeys.DomainSpecification}:Naming", "Interface names should start with an 'I'")
               // general layering rules
               .Add(t => t.That()
                   .HaveDependencyOn($"{this.baseNamespace}.Infrastructure")
                   .And().ResideInNamespace($"{this.baseNamespace}.Application")
                   .Should().HaveName("CompositionRoot"),
                   $"{LogKeys.Application}:Layering", "Application layer should not reference Infrastructure layer, except for the class named 'CompositionRoot'")
               .Add(t => t.That()
                   .ResideInNamespace($"{this.baseNamespace}.Domain")
                   .ShouldNot()
                   .HaveDependencyOn($"{this.baseNamespace}.Infrastructure"),
                   $"{LogKeys.Domain}:Layering", "Domain layer should not reference Infrastructure layer")
               .Add(t => t.That()
                   .ResideInNamespace($"{this.baseNamespace}.Infrastructure")
                   .ShouldNot()
                   .HaveDependencyOn($"{this.baseNamespace}.Application"),
                   $"{LogKeys.Infrastructure}:Layering", "Infrastructure layer should not reference Application layer")
               .Add(t => t.That()
                   .ResideInNamespace($"{this.baseNamespace}.Application").And().DoNotResideInNamespace($"{this.baseNamespace}.Application.Web")
                   .ShouldNot()
                   .HaveDependencyOn($"{this.baseNamespace}.Application.Web"),
                   $"{LogKeys.Application}:Layering", "Application layer should not reference Application.Web layer")
               .Add(t => t.That()
                   .ResideInNamespace($"{this.baseNamespace}.Application").And().DoNotResideInNamespace($"{this.baseNamespace}.Application.Console")
                   .ShouldNot()
                   .HaveDependencyOn($"{this.baseNamespace}.Application.Console"),
                   $"{LogKeys.Application}:Layering", "Application layer should not reference Application.Console layer");

            var results = policy.Evaluate();
            this.Report(results, this.output);
            results.HasViolations.ShouldBeFalse();
        }

        /// <summary>
        /// Outputs a friendly display of the policy execution results;
        /// </summary>
        /// <param name="output"><see cref="ITestOutputHelper"/> for outputs</param>
        private void Report(PolicyResults results, ITestOutputHelper output)
        {
            if (results.HasViolations)
            {
                output.WriteLine($"Policy violations found for: {results.Name}");

                foreach (var rule in results.Results)
                {
                    if (!rule.IsSuccessful)
                    {
                        output.WriteLine("-----------------------------------------------------------");
                        output.WriteLine($"{rule.Name} - {rule.Description}");
                        foreach (var type in rule.FailingTypes)
                        {
                            output.WriteLine($"\t -> {type.FullName}");
                        }
                    }
                }

                output.WriteLine("-----------------------------------------------------------");
            }
            else
            {
                output.WriteLine($"No policy violations found for: {results.Name}");
            }
        }
    }
}
