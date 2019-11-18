namespace Naos.Foundation.UnitTests.Utilities
{
    using Naos.Foundation;
    using Shouldly;
    using Xunit;
    using Xunit.Abstractions;

    public class RandomGeneratorTests : BaseTests
    {
        private readonly ITestOutputHelper output;

        public RandomGeneratorTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void WhenGeneratingMultipleInts()
        {
            var val1 = RandomGenerator.GenerateInt(100, 2000);
            var val2 = RandomGenerator.GenerateInt(100, 2000);
            var val3 = RandomGenerator.GenerateInt(100, 2000);

            val1.ShouldNotBe(val2);
            val1.ShouldNotBe(val3);
            val2.ShouldNotBe(val1);
            val2.ShouldNotBe(val3);
        }

        [Fact]
        public void WhenGeneratingAnId()
        {
            var id = RandomGenerator.GenerateComplexString(5);

            id.ShouldNotBeNullOrWhiteSpace();
            id.Length.ShouldBe(5);
        }

        [Fact]
        public void WhenGeneratingMultipleIds()
        {
            var one = RandomGenerator.GenerateComplexString(5);
            var two = RandomGenerator.GenerateComplexString(5);

            one.ShouldNotBeNullOrWhiteSpace();
            one.Length.ShouldBe(5);

            two.ShouldNotBeNullOrWhiteSpace();
            two.Length.ShouldBe(5);

            one.ShouldNotBe(two);
        }

        [Fact]
        public void WhenGeneratingMany()
        {
            this.Benchmark(() => RandomGenerator.GenerateComplexString(5), 100000, this.output);
        }

        [Fact]
        public void WhenGeneratingAnIdFast()
        {
            var id = RandomGenerator.GenerateString(5);

            id.ShouldNotBeNullOrWhiteSpace();
            id.Length.ShouldBe(5);
        }

        [Fact]
        public void WhenGeneratingMultipleIdsFast()
        {
            var one = RandomGenerator.GenerateString(5);
            var two = RandomGenerator.GenerateString(5);

            one.ShouldNotBeNullOrWhiteSpace();
            one.Length.ShouldBe(5);

            two.ShouldNotBeNullOrWhiteSpace();
            two.Length.ShouldBe(5);

            one.ShouldNotBe(two);
        }

        [Fact]
        public void WhenGeneratingManyFast()
        {
            this.Benchmark(() => RandomGenerator.GenerateString(5), 100000, this.output);
        }

        [Fact]
        public void WhenGeneratingManyPasswords()
        {
            this.Benchmark(() => RandomGenerator.GeneratePassword(12), 100000, this.output);
        }
    }
}
