namespace Naos.Core.Scheduling
{
    using Microsoft.Extensions.Logging;
    using Naos.Core.Scheduling.Domain;
    using SimpleInjector;

    public static class ContainerExtensions
    {
        public static Container AddNaosScheduling(
            this Container container,
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
                return new JobScheduler(
                    container.GetInstance<ILogger<JobScheduler>>(),
                    new SimpleInjectorJobFactory(container),
                    new InProcessMutex(container.GetInstance<ILogger<InProcessMutex>>()));
            });

            return container;
        }
    }
}
