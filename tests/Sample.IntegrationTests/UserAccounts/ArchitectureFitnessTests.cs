namespace Naos.Sample.IntegrationTests.UserAccounts
{
    using Xunit.Abstractions;

    public class ArchitectureFitnessTests : ArchitectureFitnessBaseTests
    {
        public ArchitectureFitnessTests(ITestOutputHelper output)
            : base(output, "Naos.Sample.UserAccounts")
        {
        }
    }
}
