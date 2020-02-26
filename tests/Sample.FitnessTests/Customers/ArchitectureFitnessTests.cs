namespace Naos.Sample.FitnessTests.Customers
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
