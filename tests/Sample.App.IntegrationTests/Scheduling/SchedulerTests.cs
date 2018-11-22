namespace Naos.Sample.App.IntegrationTests.Scheduling
{
    using System.Threading.Tasks;
    using Naos.Core.App.Configuration;
    using Naos.Core.App.Operations.Serilog;
    using Naos.Core.Scheduling;
    using Naos.Core.Scheduling.Domain;
    using Shouldly;
    using SimpleInjector;
    using Xunit;

    public class SchedulerTests : BaseTest
    {
        private readonly Container container = new Container();

        public SchedulerTests()
        {
            var configuration = NaosConfigurationFactory.CreateRoot();
            this.container = new Container()
                .AddNaosLogging(configuration)
                .AddNaosScheduling(configuration);
        }

        [Fact]
        public void VerifyContainer_Test()
        {
            this.container.Verify();
        }

        [Fact]
        public void CanInstantiate_Test()
        {
            var sut = this.container.GetInstance<IScheduler>();

            sut.ShouldNotBeNull();
        }

        [Fact]
        public async Task ScheduleAction_Test()
        {
            var sut = this.container.GetInstance<IScheduler>();
            var count = 0;

            sut.Register("key1", "* 12    * * * *", () =>
            {
                count++;
                System.Diagnostics.Trace.WriteLine("hello from task");
            });

            await sut.TriggerAsync("key1");
            await sut.TriggerAsync("key1");
            await sut.TriggerAsync("unk");

            count.ShouldBe(2);
        }

        [Fact]
        public async Task ScheduleFunction_Test()
        {
            var sut = this.container.GetInstance<IScheduler>();
            var count = 0;

            sut.Register("key1", "* 12    * * * *", async () =>
            await Task.Run(() =>
            {
                count++;
                System.Diagnostics.Trace.WriteLine("hello from task");
            }));

            await sut.TriggerAsync("key1");
            await sut.TriggerAsync("key1");
            await sut.TriggerAsync("unk");

            count.ShouldBe(2);
        }

        [Fact]
        public async Task ScheduleType_Test()
        {
            this.container.RegisterInstance(new StubProbe());
            var probe = this.container.GetInstance<StubProbe>();
            var sut = this.container.GetInstance<IScheduler>();

            sut.Register<StubScheduledTask>("key1", "* 12    * * * *");

            // at trigger time the StubScheduledTask (with probe in ctor) is resolved from container and executed
            await sut.TriggerAsync("key1");
            await sut.TriggerAsync("key1");
            await sut.TriggerAsync("unk");

            probe.Count.ShouldBe(2);
        }

        private class StubScheduledTask : ScheduledTask
        {
            private readonly StubProbe probe;

            public StubScheduledTask(StubProbe probe)
            {
                this.probe = probe;
            }

            public override async Task ExecuteAsync()
            {
                await Task.Run(() =>
                {
                    this.probe.Count++;
                    System.Diagnostics.Trace.WriteLine("hello from task");
                });
            }
        }

        private class StubProbe
        {
            public int Count { get; set; }
        }
    }
}
