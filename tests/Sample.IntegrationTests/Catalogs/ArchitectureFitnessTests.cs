namespace Naos.Sample.IntegrationTests.Catalogs
{
    using Xunit.Abstractions;

    public class ArchitectureFitnessTests : ArchitectureFitnessBaseTests
    {
        public ArchitectureFitnessTests(ITestOutputHelper output)
            : base(output, "Naos.Sample.Catalogs")
        {
        }
    }
}
