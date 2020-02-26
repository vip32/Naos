namespace Naos.Sample.Application.IntegrationTests.Scheduling
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Naos.Configuration.Application;
    using Naos.Foundation;
    using Naos.JobScheduling.Domain;
    using Shouldly;
    using Xunit;

    public class JobSchedulerTests : BaseTest
    {
        private readonly IJobScheduler sut;

        public JobSchedulerTests()
        {
            var configuration = NaosConfigurationFactory.Create();

            this.Services
                .AddNaos(configuration, "Product", "Capability", new[] { "All" }, n => n
                    .AddOperations(o => o
                        .AddLogging(correlationId: $"TEST{IdGenerator.Instance.Next}"))
                    .AddJobScheduling());

            this.Services.AddScoped<StubProbe>();

            this.ServiceProvider = this.Services.BuildServiceProvider();
            this.sut = this.ServiceProvider.GetRequiredService<IJobScheduler>();
        }

        [Fact]
        public void CanInstantiate_Test()
        {
            this.sut.ShouldNotBeNull();
        }

        [Fact]
        public async Task RegisterAndTriggerAction_Test()
        {
            var count = 0;

            this.sut.Options.Register("key1", "* 12 * * * *", (a) =>
            {
                count++;
                System.Diagnostics.Trace.WriteLine("+++ hello from task " + a);
            });

            var t1 = this.sut.TriggerAsync("key1");
            var t2 = this.sut.TriggerAsync("key1");

            await Task.WhenAll(new[] { t1, t2 }).AnyContext();

            count.ShouldBe(2); // action does not support overlap (due to async not supported?)
        }

        [Fact]
        public async Task RegisterAndTriggerTask_Test()
        {
            var count = 0;

            this.sut.Options.Register("key1", "* 12 * * * *", async (a) =>
                await Task.Run(() =>
                {
                    count++;
                    System.Diagnostics.Trace.WriteLine("+++ hello from task " + a);
                }).AnyContext());

            var t1 = this.sut.TriggerAsync("key1");
            var t2 = this.sut.TriggerAsync("key1");

            await Task.WhenAll(new[] { t1, t2 }).AnyContext();

            count.ShouldBe(1); // due to overlap (key1) the job runs once
        }

        [Fact]
        public async Task RegisterAndTriggerTypeWithArgs_Test()
        {
            var probe = this.ServiceProvider.GetRequiredService<StubProbe>();

            this.sut.Options.Register<StubJob>("key1", "* 12 * * * *", new[] { "once" });
            this.sut.Options.Register<StubJob>("key2", "* 12 * * * *", new[] { "once" });

            // at trigger time the StubScheduledTask (with probe in ctor) is resolved from container and executed
            var t1 = this.sut.TriggerAsync("key1");
            var t2 = this.sut.TriggerAsync("key2");

            await Task.WhenAll(new[] { t1, t2 }).AnyContext();

            probe.Count.ShouldBe(2); // each job (key1/key2) runs once (2 * 1)
        }

        [Fact]
        public async Task RegisterAndTriggerTypeWithOverlapAndArgs_Test()
        {
            var probe = this.ServiceProvider.GetRequiredService<StubProbe>();

            this.sut.Options.Register<StubJob>("key1", "* 12 * * * *", new[] { "once" });

            // at trigger time the StubScheduledTask (with probe in ctor) is resolved from container and executed
            var t1 = this.sut.TriggerAsync("key1");
            var t2 = this.sut.TriggerAsync("key1");

            await Task.WhenAll(new[] { t1, t2 }).AnyContext();

            probe.Count.ShouldBe(1); // due to overlap (key1) the job runs once
        }

        [Fact]
        public async Task RegisterAndTriggerTypeWithArgsAndCancellation_Test()
        {
            var probe = this.ServiceProvider.GetRequiredService<StubProbe>();

            this.sut.Options
                .Register<StubJob>("key1", "* 12 * * * *", new[] { "once" })
                .Register<StubJob>("key2", "* 12 * * * *", new[] { "cancel" });

            // at trigger time the StubScheduledTask (with probe in ctor) is resolved from container and executed
            var t1 = this.sut.TriggerAsync("key1");
            var t2 = this.sut.TriggerAsync("key2");

            await Task.WhenAll(new[] { t1, t2 }).AnyContext();

            probe.Count.ShouldBe(1); // jirst job trigger runs once, second job cancels directly
        }

        [Fact]
        public async Task RegisterAndTriggerTypeWithCancellation_Test()
        {
            var probe = this.ServiceProvider.GetRequiredService<StubProbe>();
            using (var cts = new CancellationTokenSource())
            {
                this.sut.Options.Register<StubJob>("key1", "* 12 * * * *");
                this.sut.Options.Register<StubJob>("key2", "* 12 * * * *");

                // at trigger time the StubScheduledTask (with probe in ctor) is resolved from container and executed
                cts.CancelAfter(TimeSpan.FromMilliseconds(250));
                var t1 = this.sut.TriggerAsync("key1", cts.Token);
                var t2 = this.sut.TriggerAsync("key2", cts.Token);
                //var t3 = sut.TriggerAsync("key3", cts.Token);

                await Task.WhenAll(new[] { t1, t2 }).AnyContext();

                probe.Count.ShouldBe(4); // every job loops twice
                                         //t1.IsCanceled.ShouldBe(true);
                                         //t2.IsCanceled.ShouldBe(true);
            }
        }

        [Fact]
        public async Task RegisterAndTriggerTypeOverlap_Test()
        {
            var probe = this.ServiceProvider.GetRequiredService<StubProbe>();

            this.sut.Options.Register<StubJob>("key1", "* 12 * * * *");

            // at trigger time the StubScheduledTask (with probe in ctor) is resolved from container and executed
            var t1 = this.sut.TriggerAsync("key1");
            var t2 = this.sut.TriggerAsync("key1"); // skipped, due to overlap
            var t3 = this.sut.TriggerAsync("unk");

            await Task.WhenAll(new[] { t1, t2, t3 }).AnyContext();

            probe.Count.ShouldBe(5);
        }

        [Fact]
        public async Task RegisterAndTriggerCustomType_Test()
        {
            var probe = this.ServiceProvider.GetRequiredService<StubProbe>();

            this.sut.Options
                .Register<StubCustomJob>("key1", "* 12 * * * *", (j) => j.MyExecuteAsync("arg1", CancellationToken.None))
                .Register<StubCustomJob>("key2", "* 12 * * * *", (j) => j.MyExecuteAsync("arg2", CancellationToken.None));

            // at trigger time the StubScheduledTask (with probe in ctor) is resolved from container and executed
            var t1 = this.sut.TriggerAsync("key1");
            var t2 = this.sut.TriggerAsync("key2");

            await Task.WhenAll(new[] { t1, t2 }).AnyContext();

            probe.Count.ShouldBe(2); // probe.count gets increased per job
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
                    var max = args.Contains("once", StringComparison.OrdinalIgnoreCase) ? 1 : 5;
                    var cancel = args.Contains("cancel", StringComparison.OrdinalIgnoreCase);

                    for (var i = 0; i < max; i++) // fake some long running process, can be cancelled with token
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        if (cancel)
                        {
                            throw new OperationCanceledException("oops");
                        }

                        this.probe.Count++;
                        System.Diagnostics.Trace.WriteLine($"+++ hello from job {DateTime.UtcNow:o}");

                        Thread.Sleep(200);
                    }
                }, cancellationToken).AnyContext();
            }
        }

        private class StubCustomJob : Job
        {
            private readonly StubProbe probe;

            public StubCustomJob(StubProbe probe)
            {
                this.probe = probe;
            }

            public Task MyExecuteAsync(string arg1, CancellationToken cancellationToken)
            {
                this.probe.Count++;
                System.Diagnostics.Trace.WriteLine($"+++ hello from custom job {DateTime.UtcNow:o} " + arg1);
                return Task.CompletedTask;
            }
        }

        private class StubProbe
        {
            public int Count { get; set; }
        }
    }
}