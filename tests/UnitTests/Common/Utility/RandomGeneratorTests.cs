namespace Naos.Core.UnitTests.Common
{
    using System;
    using System.Diagnostics;
    using Naos.Core.Common;
    using Shouldly;
    using Xunit;
    using Xunit.Abstractions;

    public class RandomGeneratorTests : BaseTest
    {
        private readonly ITestOutputHelper output;

        public RandomGeneratorTests(ITestOutputHelper output)
        {
            this.output = output;
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
