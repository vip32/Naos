namespace Naos.Sample.IntegrationTests
{
    using Naos.Application;
    using Naos.Foundation.Utilities.Xunit;
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
            // arrange
            var policy = ArchitectureFitnessPolicy.Create(this.baseNamespace);

            // act
            var results = policy.Evaluate();

            // assert
            PolicyResultsHelper.Report(results, this.output);
            results.HasViolations.ShouldBeFalse();
        }
    }
}
