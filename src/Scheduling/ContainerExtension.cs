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
            //container.RegisterSingleton<IHostedService>(() =>
            //{
            //    return new SchedulerHostedService(
            //        container.GetInstance<ILogger<SchedulerHostedService>>(),
            //        container);
            //}/*, Lifestyle.Scoped*/);

            container.RegisterSingleton<IJobScheduler>(() =>
            {
                //var logAnalyticsConfiguration = configuration.GetSection(section).Get<LogAnalyticsConfiguration>();

                return new JobScheduler(
                    container.GetInstance<ILogger<JobScheduler>>(),
                    new SimpleInjectorJobFactory(container),
                    new InProcessMutex(container.GetInstance<ILogger<InProcessMutex>>()));
            }/*, Lifestyle.Scoped*/);

            return container;
        }
    }
}
