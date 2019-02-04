namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.JobScheduling;
    using Naos.Core.JobScheduling.Domain;

    public static class ServiceExtensions
    {
        public static ServiceConfigurationContext AddJobScheduling(
        this ServiceConfigurationContext context,
        Action<JobSchedulerSettings> setupAction = null,
        string section = "naos:scheduling")
        {
            EnsureArg.IsNotNull(context, nameof(context));

            context.Services.AddSingleton<IJobScheduler>(sp =>
            {
                var settings = new JobSchedulerSettings(
                    sp.GetRequiredService<ILogger<JobSchedulerSettings>>(),
                    new ServiceProviderJobFactory(sp));
                setupAction?.Invoke(settings);

                return new JobScheduler(
                    sp.GetRequiredService<ILogger<JobScheduler>>(),
                    new InProcessMutex(sp.GetRequiredService<ILogger<InProcessMutex>>()),
                    settings);
            });

            return context;
        }
    }
}
