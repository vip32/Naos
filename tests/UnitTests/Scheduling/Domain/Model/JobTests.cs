namespace Naos.Core.UnitTests.Scheduling.Domain.Model
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Naos.Core.Scheduling.Domain;
    using Shouldly;
    using Xunit;

    public class JobTests
    {
        [Fact]
        public async Task ActionExecuteAsync_Test()
        {
            // arrange
            var count = 0;
            var sut = new Job((a) =>
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
            // arrange
            var count = 0;
            var sut = new Job(async (a) =>
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
            // arrange
            var probe = new StubProbe();
            var sut = new StubJob(probe);

            // act
            await sut.ExecuteAsync(new[] { "a" }).ConfigureAwait(false);
            await sut.ExecuteAsync(new[] { "a" }).ConfigureAwait(false);

            // assert
            probe.Count.ShouldBe(2);
        }

        [Fact]
        public async Task TypeExecuteWithCancellationAsync_Test()
        {
            // arrange
            var probe = new StubProbe();
            var cts = new CancellationTokenSource();
            var sut = new StubJob(probe);
            bool thrown = false;

            // act
            try
            {
                var t1 = sut.ExecuteAsync(cts.Token, new[] { "a" });
                cts.CancelAfter(TimeSpan.FromMilliseconds(250));

                await Task.WhenAll(new[] { t1 });
                //await ShouldThrowAsyncExtensions.ShouldThrowAsync<TaskCanceledException>(() => sut.ExecuteAsync(cancellationToken, new[] { "a" }));
            }
            catch(OperationCanceledException)
            {
                thrown = true;
            }

            // assert
            probe.Count.ShouldBe(1);
            thrown.ShouldBe(true);
        }

        private class StubJob : Job
        {
            private readonly StubProbe probe;

            public StubJob(StubProbe probe)
            {
                this.probe = probe;
            }

            public override async Task ExecuteAsync(CancellationToken cancellationToken, string[] args = null)
            {
                await Task.Run(() =>
                {
                    this.probe.Count++;
                    System.Diagnostics.Trace.WriteLine("+++ hello from job " + args);

                    for (int i = 0; i < 5; i++) // fake some long running process, can be cancelled with token
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        //if (cancellationToken.IsCancellationRequested)
                        //{
                        //    Task.FromCanceled(cancellationToken);
                        //}

                        Thread.Sleep(200);
                    }
                }, cancellationToken).ConfigureAwait(false);
            }
        }

        private class StubCustomJob
        {
            private readonly StubProbe probe;

            public StubCustomJob(StubProbe probe)
            {
                this.probe = probe;
            }

            public async Task MyExecuteAsync(string arg1, StubProbe probe, CancellationToken cancellationToken)
            {
                await Task.Run(() =>
                {
                    this.probe.Count++;
                    probe.Count++;
                    System.Diagnostics.Trace.WriteLine($"+++ hello from custom job {DateTime.UtcNow.ToString("o")} " + arg1);
                });
            }
        }

        private class StubProbe
        {
            public int Count { get; set; }
        }
    }
}
