namespace Naos.Sample.IntegrationTests.Customers
{
    using Xunit.Abstractions;

    public class ArchitectureFitnessTests : ArchitectureFitnessBaseTests
    {
        public ArchitectureFitnessTests(ITestOutputHelper output)
            : base(output, "Naos.Sample.Customers")
        {
        }
    }
}
