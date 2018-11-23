namespace Naos.Core.UnitTests.Scheduling.Domain.Model
{
    using System.Threading.Tasks;
    using Naos.Core.Scheduling.Domain;
    using Shouldly;
    using Xunit;

    public class ScheduledTaskTests
    {
        [Fact]
        public async Task ActionExecuteAsync_Test()
        {
            // arrang
            var count = 0;
            var sut = new ScheduledTask((a) =>
            {
                count++;
                System.Diagnostics.Trace.WriteLine("hello from task " + a);
            });

            // act
            await sut.ExecuteAsync(new[] { "a" }).ConfigureAwait(false);
            await sut.ExecuteAsync(new[] { "a" }).ConfigureAwait(false);

            // assert
            count.ShouldBe(2);
        }

        [Fact]
        public async Task FuncExecuteAsync_Test()
        {
            // arrang
            var count = 0;
            var sut = new ScheduledTask(async (a) =>
                await Task.Run(() =>
                {
                    count++;
                    System.Diagnostics.Trace.WriteLine("hello from task " + a);
                }));

            // act
            await sut.ExecuteAsync(new[] { "a" }).ConfigureAwait(false);
            await sut.ExecuteAsync(new[] { "a" }).ConfigureAwait(false);

            // assert
            count.ShouldBe(2);
        }

        [Fact]
        public async Task TypeExecuteAsync_Test()
        {
            // arrang
            var probe = new StubProbe();
            var sut = new StubScheduledTask(probe);

            // act
            await sut.ExecuteAsync(new[] { "a" }).ConfigureAwait(false);
            await sut.ExecuteAsync(new[] { "a" }).ConfigureAwait(false);

            // assert
            probe.Count.ShouldBe(2);
        }

        private class StubScheduledTask : ScheduledTask
        {
            private readonly StubProbe probe;

            public StubScheduledTask(StubProbe probe)
            {
                this.probe = probe;
            }

            public override async Task ExecuteAsync(string[] args = null)
            {
                await Task.Run(() =>
                {
                    this.probe.Count++;
                    System.Diagnostics.Trace.WriteLine("hello from task " + args);
                    System.Threading.Thread.Sleep(1000);
                });
            }
        }

        private class StubProbe
        {
            public int Count { get; set; }
        }
    }
}
