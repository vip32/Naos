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

    public static class ServiceExtensions
    {
        public static INaosBuilderContext AddJobScheduling(
        this INaosBuilderContext context,
        Action<JobSchedulerOptions> setupAction = null,
        string section = "naos:scheduling")
        {
            EnsureArg.IsNotNull(context, nameof(context));

            context.Services.AddSingleton<IJobScheduler>(sp =>
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

            context.Services.AddSingleton<IHostedService>(sp =>
                new JobSchedulerHostedService(sp.GetRequiredService<ILogger<JobSchedulerHostedService>>(), sp));

            context.Messages.Add($"{LogEventKeys.General} naos builder: job scheduling added"); // TODO: list available commands/handlers

            return context;
        }
    }
}
