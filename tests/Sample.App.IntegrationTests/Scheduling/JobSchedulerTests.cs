namespace Naos.Sample.App.IntegrationTests.Scheduling
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Naos.Core.Common;
    using Naos.Core.Configuration;
    using Naos.Core.JobScheduling.Domain;
    using Shouldly;
    using Xunit;

    public class JobSchedulerTests : BaseTest
    {
        private readonly IServiceCollection services = new ServiceCollection();

        public JobSchedulerTests()
        {
            var configuration = NaosConfigurationFactory.Create();

            this.services
                .Naos(configuration, "Product", "Capability")
                    .AddOperations(o => o
                        .AddLogging(correlationId: $"TEST{RandomGenerator.GenerateString(9, true)}"))
                    .AddJobScheduling();

            this.services.AddScoped<StubProbe>();

            this.ServiceProvider = this.services.BuildServiceProvider();
        }

        public ServiceProvider ServiceProvider { get; private set; }

        //[Fact]
        //public void VerifyContainer_Test()
        //{
        //    this.container.Verify();
        //}

        [Fact]
        public void CanInstantiate_Test()
        {
            var sut = this.ServiceProvider.GetRequiredService<IJobScheduler>();

            sut.ShouldNotBeNull();
        }

        [Fact]
        public async Task RegisterAndTriggerAction_Test()
        {
            var sut = this.ServiceProvider.GetRequiredService<IJobScheduler>();
            var count = 0;

            sut.Options.Register("key1", "* 12 * * * *", (a) =>
            {
                count++;
                System.Diagnostics.Trace.WriteLine("+++ hello from task " + a);
            });

            var t1 = sut.TriggerAsync("key1");
            var t2 = sut.TriggerAsync("key1");

            await Task.WhenAll(new[] { t1, t2 });

            count.ShouldBe(2); // action does not support overlap (due to async not supported?)
        }

        [Fact]
        public async Task RegisterAndTriggerTask_Test()
        {
            var sut = this.ServiceProvider.GetRequiredService<IJobScheduler>();
            var count = 0;

            sut.Options.Register("key1", "* 12 * * * *", async (a) =>
                await Task.Run(() =>
                {
                    count++;
                    System.Diagnostics.Trace.WriteLine("+++ hello from task " + a);
                }));

            var t1 = sut.TriggerAsync("key1");
            var t2 = sut.TriggerAsync("key1");

            await Task.WhenAll(new[] { t1, t2 });

            count.ShouldBe(1); // due to overlap (key1) the job runs once
        }

        [Fact]
        public async Task RegisterAndTriggerTypeWithArgs_Test()
        {
            var probe = this.ServiceProvider.GetRequiredService<StubProbe>();
            var sut = this.ServiceProvider.GetRequiredService<IJobScheduler>();

            sut.Options.Register<StubJob>("key1", "* 12 * * * *", new[] { "once" });
            sut.Options.Register<StubJob>("key2", "* 12 * * * *", new[] { "once" });

            // at trigger time the StubScheduledTask (with probe in ctor) is resolved from container and executed
            var t1 = sut.TriggerAsync("key1");
            var t2 = sut.TriggerAsync("key2");

            await Task.WhenAll(new[] { t1, t2 });

            probe.Count.ShouldBe(2); // each job (key1/key2) runs once (2 * 1)
        }

        [Fact]
        public async Task RegisterAndTriggerTypeWithOverlapAndArgs_Test()
        {
            var probe = this.ServiceProvider.GetRequiredService<StubProbe>();
            var sut = this.ServiceProvider.GetRequiredService<IJobScheduler>();

            sut.Options.Register<StubJob>("key1", "* 12 * * * *", new[] { "once"});

            // at trigger time the StubScheduledTask (with probe in ctor) is resolved from container and executed
            var t1 = sut.TriggerAsync("key1");
            var t2 = sut.TriggerAsync("key1");

            await Task.WhenAll(new[] { t1, t2 });

            probe.Count.ShouldBe(1); // due to overlap (key1) the job runs once
        }

        [Fact]
        public async Task RegisterAndTriggerTypeWithArgsAndCancellation_Test()
        {
            var probe = this.ServiceProvider.GetRequiredService<StubProbe>();
            var sut = this.ServiceProvider.GetRequiredService<IJobScheduler>();

            sut.Options
                .Register<StubJob>("key1", "* 12 * * * *", new[] { "once" })
                .Register<StubJob>("key2", "* 12 * * * *", new[] { "cancel" });

            // at trigger time the StubScheduledTask (with probe in ctor) is resolved from container and executed
            var t1 = sut.TriggerAsync("key1");
            var t2 = sut.TriggerAsync("key2");

            await Task.WhenAll(new[] { t1, t2 });

            probe.Count.ShouldBe(1); // jirst job trigger runs once, second job cancels directly
        }

        [Fact]
        public async Task RegisterAndTriggerTypeWithCancellation_Test()
        {
            var probe = this.ServiceProvider.GetRequiredService<StubProbe>();
            var cts = new CancellationTokenSource();
            var sut = this.ServiceProvider.GetRequiredService<IJobScheduler>();

            sut.Options.Register<StubJob>("key1", "* 12 * * * *");
            sut.Options.Register<StubJob>("key2", "* 12 * * * *");

            // at trigger time the StubScheduledTask (with probe in ctor) is resolved from container and executed
            cts.CancelAfter(TimeSpan.FromMilliseconds(250));
            var t1 = sut.TriggerAsync("key1", cts.Token);
            var t2 = sut.TriggerAsync("key2", cts.Token);
            //var t3 = sut.TriggerAsync("key3", cts.Token);

            await Task.WhenAll(new[] { t1, t2 });

            probe.Count.ShouldBe(4); // every job loops twice
            //t1.IsCanceled.ShouldBe(true);
            //t2.IsCanceled.ShouldBe(true);
        }

        [Fact]
        public async Task RegisterAndTriggerTypeOverlap_Test()
        {
            var probe = this.ServiceProvider.GetRequiredService<StubProbe>();
            var sut = this.ServiceProvider.GetRequiredService<IJobScheduler>();

            sut.Options.Register<StubJob>("key1", "* 12 * * * *");

            // at trigger time the StubScheduledTask (with probe in ctor) is resolved from container and executed
            var t1 = sut.TriggerAsync("key1");
            var t2 = sut.TriggerAsync("key1"); // skipped, due to overlap
            var t3 = sut.TriggerAsync("unk");

            await Task.WhenAll(new[] { t1, t2, t3 });

            probe.Count.ShouldBe(5);
        }

        [Fact]
        public async Task RegisterAndTriggerCustomType_Test()
        {
            var probe = this.ServiceProvider.GetRequiredService<StubProbe>();
            var sut = this.ServiceProvider.GetRequiredService<IJobScheduler>();

            sut.Options
                .Register<StubCustomJob>("key1", "* 12 * * * *", (j) => j.MyExecuteAsync("arg1", probe, CancellationToken.None))
                .Register<StubCustomJob>("key2", "* 12 * * * *", (j) => j.MyExecuteAsync("arg2", probe, CancellationToken.None));

            // at trigger time the StubScheduledTask (with probe in ctor) is resolved from container and executed
            var t1 = sut.TriggerAsync("key1");
            var t2 = sut.TriggerAsync("key2");

            await Task.WhenAll(new[] { t1, t2 });

            probe.Count.ShouldBe(4); // probe.count gets increased per job
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

                    for (int i = 0; i < max; i++) // fake some long running process, can be cancelled with token
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        if (cancel)
                        {
                            throw new OperationCanceledException("oops");
                        }

                        this.probe.Count++;
                        System.Diagnostics.Trace.WriteLine($"+++ hello from job {DateTime.UtcNow.ToString("o")}");

                        Thread.Sleep(200);
                    }
                }, cancellationToken);
            }
        }

        private class StubCustomJob : Job
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