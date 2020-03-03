namespace Naos.Application
{
    using Microsoft.Extensions.Logging;
    using Naos.Commands.Application;
    using Naos.Foundation;
    using Naos.Foundation.Domain;
    using Naos.Messaging.Domain;
    using NetArchTest.Rules;
    using NetArchTest.Rules.Policies;

    public static class ArchitectureFitnessPolicy
    {
        public static PolicyDefinition Create(string baseNamespace)
        {
            return Policy.Define("Architecture Fitness Policy", "This policy contains a valid passing policy based on Domain Driven Design guidelines")
               .For(Types.InNamespace(baseNamespace))
               // application messaging rules
               .Add(t => t.That()
                   .Inherit(typeof(Message)).Or().ImplementInterface(typeof(IMessageHandler<>))
                   .Should().ResideInNamespace($"{baseNamespace}.Application"),
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
                   .Should().ResideInNamespace($"{baseNamespace}.Application"),
                   $"{LogKeys.AppCommand}:Layering", "Commands should only exist in the Application layer")
               .Add(t => t.That()
                   /*.Inherit(typeof(Command)).Or()*/.Inherit(typeof(Command<>))
                   .Should().HaveNameEndingWith("Command").Or().HaveNameEndingWith("Query"),
                   $"{LogKeys.AppCommand}:Naming", "Commands or Queries should be named correctly")
               .Add(t => t.That()
                   .Inherit(typeof(CommandHandler<,>))
                   .Should().ResideInNamespace($"{baseNamespace}.Application"),
                   $"{LogKeys.AppCommand}:Layering", "CommandHandlers should only exist in the Application layer")
               .Add(t => t.That()
                   .Inherit(typeof(CommandHandler<,>))
                   .Should().HaveNameEndingWith("CommandHandler").Or().HaveNameEndingWith("QueryHandler"),
                   $"{LogKeys.AppCommand}:Layering", "CommandHandlers should only exist in the Application layer")
               .Add(t => t.That()
                   .HaveNameEndingWith("Controller")
                   .Should().ResideInNamespace($"{baseNamespace}.Application.Web"),
                   $"{LogKeys.DomainEvent}:Layering", "Controllers should only exist in the Application.Web layer")
               .Add(t => t.That()
                   .HaveNameEndingWith("ConsoleCommand")
                   .Should().ResideInNamespace($"{baseNamespace}.Application"),
                   $"{LogKeys.DomainEvent}:Layering", "ConsoleCommands should only exist in the Application layer")
               .Add(t => t.That()
                   .ImplementInterface(typeof(IStartupTask))
                   .Should().ResideInNamespace($"{baseNamespace}.Application"),
                   $"{LogKeys.DomainEvent}:Layering", "ConsoleCommands should only exist in the Application layer")
               // domain event rules
               .Add(t => t.That()
                   .Inherit(typeof(Entity<>))
                   .And().DoNotInherit(typeof(Command<>)) // ???? somehow Commands are selected as Entities
                   .Should().ResideInNamespace($"{baseNamespace}.Domain"),
                   $"{LogKeys.DomainEvent}:Layering", "Domain Entities should only exist in the Domain layer")
               .Add(t => t.That()
                   .Inherit(typeof(AggregateRoot<>))
                   .Should().ResideInNamespace($"{baseNamespace}.Domain"),
                   $"{LogKeys.DomainEvent}:Layering", "Domain Aggregates should only exist in the Domain layer")
               .Add(t => t.That()
                   .ImplementInterface(typeof(IDomainEvent))
                   .Should().ResideInNamespace($"{baseNamespace}.Domain"),
                   $"{LogKeys.DomainEvent}:Layering", "DomainEvents should only exist in the Domain layer")
               .Add(t => t.That()
                   .ImplementInterface(typeof(IDomainEventHandler<>))
                   .Should().ResideInNamespace($"{baseNamespace}.Domain"),
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
                    .Should().ResideInNamespace($"{baseNamespace}.Domain").Or().ResideInNamespace($"{baseNamespace}.Infrastructure"),
                    $"{LogKeys.DomainRepository}:Layering", "Classes that implement IGenericRepository must reside in the Domain or Infrastructure namespace")
               .Add(t => t.That()
                    .ImplementInterface(typeof(IReadOnlyGenericRepository<>))
                    .Should().ResideInNamespace($"{baseNamespace}.Domain").Or().ResideInNamespace($"{baseNamespace}.Infrastructure"),
                    $"{LogKeys.DomainRepository}:Layering", "Classes that implement IReadOnlyGenericRepository must reside in the Domain or Infrastructure namespace")
               .Add(t => t.That()
                    .ImplementInterface(typeof(IGenericRepository<>)).Or().ImplementInterface(typeof(IReadOnlyGenericRepository<>))
                    .Should().HaveNameEndingWith("Repository"),
                    $"{LogKeys.DomainRepository}:Naming", "Repositories should be named correctly")
               // domain specification rules
               .Add(t => t.That()
                   .ImplementInterface(typeof(ISpecification<>))
                   .Should().ResideInNamespace($"{baseNamespace}.Domain"),
                   $"{LogKeys.DomainSpecification}:Layering", "Specifications should only exist in the Domain layer")
               .Add(t => t.That()
                   .ImplementInterface(typeof(ISpecification<>))
                   .Should().HaveNameEndingWith("Specification"),
                   $"{LogKeys.DomainSpecification}:Naming", "Specifications should be named correctly")
               .Add(t => t.That()
                   .Inherit(typeof(ValueObject))
                   .Should().ResideInNamespace($"{baseNamespace}.Domain"),
                   $"{LogKeys.AppCommand}:Layering", "ValueObjects should only exist in the Domain layer")
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
                   .HaveDependencyOn($"{baseNamespace}.Infrastructure")
                   .And().ResideInNamespace($"{baseNamespace}.Application")
                   .Should().HaveName("CompositionRoot"),
                   $"{LogKeys.Application}:Layering", "Application layer should not reference Infrastructure layer, except for the class named 'CompositionRoot'")
               .Add(t => t.That()
                   .ResideInNamespace($"{baseNamespace}.Domain")
                   .ShouldNot()
                   .HaveDependencyOn($"{baseNamespace}.Infrastructure"),
                   $"{LogKeys.Domain}:Layering", "Domain layer should not reference Infrastructure layer")
               .Add(t => t.That()
                   .ResideInNamespace($"{baseNamespace}.Infrastructure")
                   .ShouldNot()
                   .HaveDependencyOn($"{baseNamespace}.Application"),
                   $"{LogKeys.Infrastructure}:Layering", "Infrastructure layer should not reference Application layer")
               .Add(t => t.That()
                   .ResideInNamespace($"{baseNamespace}.Application").And().DoNotResideInNamespace($"{baseNamespace}.Application.Web")
                   .ShouldNot()
                   .HaveDependencyOn($"{baseNamespace}.Application.Web"),
                   $"{LogKeys.Application}:Layering", "Application layer should not reference Application.Web layer")
               .Add(t => t.That()
                   .ResideInNamespace($"{baseNamespace}.Application").And().DoNotResideInNamespace($"{baseNamespace}.Application.Console")
                   .ShouldNot()
                   .HaveDependencyOn($"{baseNamespace}.Application.Console"),
                   $"{LogKeys.Application}:Layering", "Application layer should not reference Application.Console layer");
        }
    }
}
