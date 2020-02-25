namespace Naos.Sample.FitnessTests
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
        protected virtual void ArchitectureFitnessPolicyTest()
        {
            // arrange
            var policy = ArchitectureFitnessPolicy.Create("Naos.Sample.UserAccounts");

            // act
            var results = policy.Evaluate();

            // assert
            PolicyResultsReporter.Write(results, this.output);
            results.HasViolations.ShouldBeFalse();
        }
    }
}
