namespace Naos.Foundation.UnitTests.Utilities
{
    using Naos.Foundation;
    using Shouldly;
    using Xunit;

    public class SystemMetricsClientTests
    {
        [Fact]
        public void MemoryTest()
        {
            // arrange
            var sut = new SystemMetricsClient();

            // act
            var result = sut.GetMemoryMetrics();

            // assert
            result.Total.ShouldBeGreaterThan(0);
            result.Used.ShouldBeGreaterThan(0);
            result.Free.ShouldBeGreaterThan(0);
        }

        [Fact]
        public void CpuTest()
        {
            // arrange
            var sut = new SystemMetricsClient();

            // act
            var result = sut.GetCpuMetrics();

            // assert
            result.LoadPercentage.ShouldBeGreaterThan(0);
        }
    }
}
