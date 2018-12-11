namespace Naos.Core.Scheduling
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Scheduling.App;
    using Naos.Core.Scheduling.Domain;
    using SimpleInjector;

    public static class ServiceRegistrations
    {
        public static Container AddNaosScheduling(
            this Container container,
            IServiceCollection services = null,
            string section = "naos:scheduling")
        {
            //container.RegisterSingleton<IHostedService>(() =>
            //{
            //    return new SchedulerHostedService(
            //        container.GetInstance<ILogger<SchedulerHostedService>>(),
            //        container);
            //}/*, Lifestyle.Scoped*/);

            // TODO: temporary solution to get the scheduler hosted service to run (with its dependencies)
            // https://stackoverflow.com/questions/50394666/injecting-simple-injector-components-into-ihostedservice-with-asp-net-core-2-0#
            services?.AddSingleton<Microsoft.Extensions.Hosting.IHostedService>(sp =>
                    new JobSchedulerHostedService(sp.GetService<ILogger<JobSchedulerHostedService>>(), container));

            container.RegisterSingleton<IJobScheduler>(() =>
            {
                return new JobScheduler(
                    container.GetInstance<ILogger<JobScheduler>>(),
                    new SimpleInjectorJobFactory(container),
                    new InProcessMutex(container.GetInstance<ILogger<InProcessMutex>>()));
            });

            //services?.AddSingleton<IJobScheduler>(sp =>
            //{
            //    return new JobScheduler(
            //        container.GetInstance<ILogger<JobScheduler>>(),
            //        new ServiceProviderJobFactory(sp),
            //        new InProcessMutex(container.GetInstance<ILogger<InProcessMutex>>()));
            //});

            return container;
        }
    }
}
