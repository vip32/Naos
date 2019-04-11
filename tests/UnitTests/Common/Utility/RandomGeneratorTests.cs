namespace Naos.Core.UnitTests.Common
{
    using System;
    using System.Diagnostics;
    using Naos.Core.Common;
    using Shouldly;
    using Xunit;
    using Xunit.Abstractions;

    public class RandomGeneratorTests
    {
        private readonly ITestOutputHelper output;

        public RandomGeneratorTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void WhenGeneratingAnId()
        {
            var id = RandomGenerator.GenerateString(5);

            id.ShouldNotBeNullOrWhiteSpace();
            id.Length.ShouldBe(5);
        }

        [Fact]
        public void WhenGeneratingMultipleIds()
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
        public void WhenGeneratingMany()
        {
            var sw = Stopwatch.StartNew();
            for(var i = 0; i < 10000; i++)
            {
                RandomGenerator.GenerateString(5);
            }

            sw.Stop();

            using(var process = Process.GetCurrentProcess())
            {
                this.output.WriteLine("execution time: {0}ms\r\n  - Gen-0: {1}, Gen-1: {2}, Gen-2: {3}",
                        sw.Elapsed.TotalMilliseconds.ToString(),
                        GC.CollectionCount(0),
                        GC.CollectionCount(1),
                        GC.CollectionCount(2));
            }
        }

        [Fact]
        public void WhenGeneratingAnIdFast()
        {
            var id = RandomGenerator.GenerateStringFast(5);

            id.ShouldNotBeNullOrWhiteSpace();
            id.Length.ShouldBe(5);
        }

        [Fact]
        public void WhenGeneratingMultipleIdsFast()
        {
            var one = RandomGenerator.GenerateStringFast(5);
            var two = RandomGenerator.GenerateStringFast(5);

            one.ShouldNotBeNullOrWhiteSpace();
            one.Length.ShouldBe(5);

            two.ShouldNotBeNullOrWhiteSpace();
            two.Length.ShouldBe(5);

            one.ShouldNotBe(two);
        }

        [Fact]
        public void WhenGeneratingManyFast()
        {
            var sw = Stopwatch.StartNew();
            for(var i = 0; i < 10000; i++)
            {
                RandomGenerator.GenerateStringFast(5);
            }

            sw.Stop();

            using(var process = Process.GetCurrentProcess())
            {
                this.output.WriteLine("execution time: {0}ms\r\n  - Gen-0: {1}, Gen-1: {2}, Gen-2: {3}",
                        sw.Elapsed.TotalMilliseconds.ToString(),
                        GC.CollectionCount(0),
                        GC.CollectionCount(1),
                        GC.CollectionCount(2));
            }
        }
    }
}
