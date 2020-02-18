namespace Naos.Sample.IntegrationTests.Inventory
{
    using Xunit.Abstractions;

    public class ArchitectureFitnessTests : ArchitectureFitnessBaseTests
    {
        public ArchitectureFitnessTests(ITestOutputHelper output)
            : base(output, "Naos.Sample.Inventory")
        {
        }
    }
}
