namespace Naos.Core.UnitTests.Common
{
    using System;
    using System.Diagnostics;
    using Naos.Core.Common;
    using Shouldly;
    using Xunit;
    using Xunit.Abstractions;

    public class IdGeneratorTests
    {
        private readonly ITestOutputHelper output;

        public IdGeneratorTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void WhenCreatingMultipleInstancesOfTheGenerator()
        {
            var one = IdGenerator.Instance;
            var two = IdGenerator.Instance;

            one.ShouldBeSameAs(two);
        }

        [Fact]
        public void WhenGeneratingAnId()
        {
            var id = IdGenerator.Instance.Next;

            id.ShouldNotBeNullOrWhiteSpace();
            id.Length.ShouldBe(20);
        }

        [Fact]
        public void WhenGeneratingMultipleIds()
        {
            var one = IdGenerator.Instance.Next;
            var two = IdGenerator.Instance.Next;

            one.ShouldNotBeNullOrWhiteSpace();
            one.Length.ShouldBe(20);

            two.ShouldNotBeNullOrWhiteSpace();
            two.Length.ShouldBe(20);

            two.ShouldBeGreaterThan(one);
            one.ShouldNotBe(two);
            one.Substring(0, 7).ShouldBe(two.Substring(0, 7));
        }

        [Fact]
        public void WhenGeneratingMany()
        {
            var sw = Stopwatch.StartNew();
            for(var i = 0; i < 10000; i++)
            {
                _ = IdGenerator.Instance.Next;
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
