namespace Naos.Core.Scheduling
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Scheduling.Domain;
    using SimpleInjector;

    public static class ContainerExtension
    {
        public static Container AddNaosScheduling(
            this Container container,
            IConfiguration configuration,
            string section = "naos:scheduling")
        {
            container.Register<IScheduler>(() =>
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
