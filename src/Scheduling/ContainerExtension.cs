namespace Naos.Core.Scheduling
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Scheduling.App;
    using Naos.Core.Scheduling.Domain;
    using SimpleInjector;

    public static class ContainerExtension
    {
        public static Container AddNaosScheduling(
            this Container container,
            IConfiguration configuration,
            string section = "naos:scheduling")
        {
            //container.RegisterSingleton<IHostedService>(() =>
            //{
            //    return new SchedulerHostedService(
            //        container.GetInstance<ILogger<SchedulerHostedService>>(),
            //        container);
            //}/*, Lifestyle.Scoped*/);

            container.RegisterSingleton<IScheduler>(() =>
            {
                //var logAnalyticsConfiguration = configuration.GetSection(section).Get<LogAnalyticsConfiguration>();

                return new Scheduler(
                    container.GetInstance<ILogger<Scheduler>>(),
                    new SimpleInjectorScheduledTaskFactory(container),
                    new InMemoryMutex());
            }/*, Lifestyle.Scoped*/);

            return container;
        }
    }
}
