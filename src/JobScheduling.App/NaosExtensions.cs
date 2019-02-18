namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using EnsureThat;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.JobScheduling;
    using Naos.Core.JobScheduling.App;
    using Naos.Core.JobScheduling.Domain;

    public static class NaosExtensions
    {
        public static NaosOptions AddJobScheduling(
        this NaosOptions naosOptions,
        Action<JobSchedulerOptions> setupAction = null,
        string section = "naos:scheduling")
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));
            EnsureArg.IsNotNull(naosOptions.Context, nameof(naosOptions.Context));

            naosOptions.Context.Services.AddSingleton<IJobScheduler>(sp =>
            {
                var settings = new JobSchedulerOptions(
                    sp.GetRequiredService<ILoggerFactory>(),
                    new ServiceProviderJobFactory(sp));
                setupAction?.Invoke(settings);

                return new JobScheduler(
                    sp.GetRequiredService<ILoggerFactory>(),
                    new InProcessMutex(sp.GetRequiredService<ILogger<InProcessMutex>>()),
                    settings);
            });

            naosOptions.Context.Services.AddSingleton<IHostedService>(sp =>
                new JobSchedulerHostedService(sp.GetRequiredService<ILogger<JobSchedulerHostedService>>(), sp));

            naosOptions.Context.Messages.Add($"{LogEventKeys.Startup} naos builder: job scheduling added"); // TODO: list available commands/handlers

            return naosOptions;
        }
    }
}
