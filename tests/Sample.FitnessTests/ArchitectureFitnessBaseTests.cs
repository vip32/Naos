namespace Naos.Sample.FitnessTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Naos.Application;
    using Naos.Commands.Application;
    using Naos.Foundation.Domain;
    using Naos.Foundation.Utilities.Xunit;
    using NetArchTest.Rules;
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
        public virtual void ArchitectureFitnessPolicyTest()
        {
            // arrange
            var policy = ArchitectureFitnessPolicy.Create(this.baseNamespace);

            // act
            var results = policy.Evaluate();

            // assert
            PolicyResultsReporter.Write(results, this.output);
            results.HasViolations.ShouldBeFalse();
        }

        [Fact]
        [FitnessTest]
        [Trait("Category", "Fitness")]
        public void Domain_Event_Should_Be_Immutable()
        {
            var types = Types.InNamespace(this.baseNamespace)
                .That()
                .ImplementInterface(typeof(IDomainEvent))
                .Or()
                .Inherit(typeof(DomainEvent)).GetTypes();

            AssertAreImmutable(types);
        }

        [Fact]
        [FitnessTest]
        [Trait("Category", "Fitness")]
        public void Domain_ValueObjects_Should_Be_Immutable()
        {
            var types = Types.InNamespace(this.baseNamespace)
                .That()
                .Inherit(typeof(ValueObject)).GetTypes();

            AssertAreImmutable(types);
        }

        //[Fact]
        //[FitnessTest]
        //[Trait("Category", "Fitness")]
        //public void Domain_Entity_Should_Have_Only_Private_Constructors()
        //{
        //    var types = Types.InNamespace(this.baseNamespace)
        //        .That()
        //        .Inherit(typeof(AggregateRoot<>))
        //        .Or()
        //        .Inherit(typeof(Entity<>))
        //        .GetTypes();

        //    var results = new List<Type>();
        //    foreach (var type in types)
        //    {
        //        var constructors = type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        //        foreach (var constructorInfo in constructors)
        //        {
        //            if (!constructorInfo.IsPrivate)
        //            {
        //                results.Add(type);
        //            }
        //        }
        //    }

        //    results.ShouldBeEmpty();
        //}

        public void Domain_Entity_Should_Have_Parameterless_Private_Constructor()
        {
            var types = Types.InNamespace(this.baseNamespace)
              .That()
              .Inherit(typeof(Entity<>)).GetTypes();

            var results = new List<Type>();
            foreach (var type in types)
            {
                var hasPrivateParameterlessConstructor = false;
                var constructors = type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var constructorInfo in constructors)
                {
                    if (constructorInfo.IsPrivate && constructorInfo.GetParameters().Length == 0)
                    {
                        hasPrivateParameterlessConstructor = true;
                    }
                }

                if (!hasPrivateParameterlessConstructor)
                {
                    results.Add(type);
                }
            }

            results.ShouldBeEmpty();
        }

        [Fact]
        [FitnessTest]
        [Trait("Category", "Fitness")]
        public void Domain_ValueObject_Should_Have_Private_Constructor_With_Parameters_For_His_State()
        {
            var valueObjects = Types.InNamespace(this.baseNamespace)
                .That()
                .Inherit(typeof(ValueObject)).GetTypes();

            var results = new List<Type>();
            foreach (var entityType in valueObjects)
            {
                var hasExpectedConstructor = false;

                const BindingFlags bindingFlags = BindingFlags.DeclaredOnly |
                                                  BindingFlags.Public |
                                                  BindingFlags.Instance;
                var names = entityType.GetFields(bindingFlags).Select(x => x.Name.ToLower()).ToList();
                var propertyNames = entityType.GetProperties(bindingFlags).Select(x => x.Name.ToLower()).ToList();
                names.AddRange(propertyNames);
                var constructors = entityType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var constructorInfo in constructors)
                {
                    var parameters = constructorInfo.GetParameters().Select(x => x.Name.ToLower()).ToList();

                    if (names.Intersect(parameters).Count() == names.Count)
                    {
                        hasExpectedConstructor = true;
                        break;
                    }
                }

                if (!hasExpectedConstructor)
                {
                    results.Add(entityType);
                }
            }

            results.ShouldBeEmpty();
        }

        [Fact]
        [FitnessTest]
        [Trait("Category", "Fitness")]
        public virtual void Domain_Entity_Or_AggregateRoot_Cannot_Have_Reference_To_Other_AggregateRoot() // use a reference id instead https://enterprisecraftsmanship.com/posts/link-to-an-aggregate-reference-or-id/
        {
            var entityTypes = Types.InNamespace(this.baseNamespace)
                .That()
                .Inherit(typeof(Entity<>))
                .And()
                .DoNotInherit(typeof(Command<>)).GetTypes(); // ???? somehow Commands are selected as Entities
                //.And()
                //.DoNotInherit(typeof(AggregateRoot<>)).GetTypes();

            var aggregateRoots = Types.InNamespace(this.baseNamespace)
                .That()
                .Inherit(typeof(AggregateRoot<>)).GetTypes().ToList();

            const BindingFlags bindingFlags = BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance;
            var results = new List<Type>();

            foreach (var type in entityTypes)
            {
                var fields = type.GetFields(bindingFlags);
                foreach (var field in fields)
                {
                    if (aggregateRoots.Contains(field.FieldType) ||
                        field.FieldType.GenericTypeArguments.Any(x => aggregateRoots.Contains(x)))
                    {
                        results.Add(type);
                        break;
                    }
                }

                var properties = type.GetProperties(bindingFlags);
                foreach (var property in properties)
                {
                    if (aggregateRoots.Contains(property.PropertyType) ||
                        property.PropertyType.GenericTypeArguments.Any(x => aggregateRoots.Contains(x)))
                    {
                        results.Add(type);
                        break;
                    }
                }
            }

            results.ShouldBeEmpty();
        }

        protected static void AssertAreImmutable(IEnumerable<Type> types)
        {
            var results = new List<Type>();
            foreach (var type in types)
            {
                if (type.GetFields().Any(x => !x.IsInitOnly) || type.GetProperties(BindingFlags.Public).Any(x => x.CanWrite))
                {
                    results.Add(type);
                    break;
                }
            }

            results.ShouldBeEmpty();
        }
    }
}