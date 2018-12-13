namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Scheduling;
    using Naos.Core.Scheduling.App;
    using Naos.Core.Scheduling.Domain;

    public static class ServiceRegistrations
    {
        public static IServiceCollection AddNaosScheduling(
        this IServiceCollection services,
        Action<JobSchedulerSettings> setupAction = null,
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
            services.AddSingleton<Hosting.IHostedService>(sp =>
                    new JobSchedulerHostedService(sp.GetRequiredService<ILogger<JobSchedulerHostedService>>(), sp));

            //container.RegisterSingleton<IJobScheduler>(() =>
            //{
            //    var settings = new JobSchedulerSettings(
            //        container.GetInstance<ILogger<JobSchedulerSettings>>(),
            //        new SimpleInjectorJobFactory(container));
            //    setupAction?.Invoke(settings);

            //    var result = new JobScheduler(
            //        container.GetInstance<ILogger<JobScheduler>>(),
            //        new InProcessMutex(container.GetInstance<ILogger<InProcessMutex>>()),
            //        settings);

            //    return result;
            //});

            services.AddSingleton<IJobScheduler>(sp =>
            {
                var settings = new JobSchedulerSettings(
                    sp.GetRequiredService<ILogger<JobSchedulerSettings>>(),
                    new ServiceProviderJobFactory(sp));
                setupAction?.Invoke(settings);

                var result = new JobScheduler(
                    sp.GetRequiredService<ILogger<JobScheduler>>(),
                    new InProcessMutex(sp.GetRequiredService<ILogger<InProcessMutex>>()),
                    settings);

                return result;
            });

            return services;
        }
    }
}
