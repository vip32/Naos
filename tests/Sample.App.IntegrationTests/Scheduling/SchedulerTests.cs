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
        public async Task Test1()
        {
            var sut = this.container.GetInstance<IScheduler>();
            var count = 0;

            sut.Register("key1", "* 12    * * * *", () =>
            {
                count++;
                System.Diagnostics.Trace.WriteLine("hello from task");
            });

            sut.Register("key2", "* 12    * * * *", async () =>
            await Task.Run(() =>
            {
                count++;
                System.Diagnostics.Trace.WriteLine("hello from task");
            }));

            await sut.TriggerAsync("key1");
            await sut.TriggerAsync("key2");
            await sut.TriggerAsync("key3");

            count.ShouldBe(2);
        }
    }
}
