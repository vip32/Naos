namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using EnsureThat;
    using MediatR;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Configuration.App;
    using Naos.Core.JobScheduling;
    using Naos.Core.JobScheduling.App;
    using Naos.Core.JobScheduling.Domain;
    using Naos.Foundation;

    [ExcludeFromCodeCoverage]
    public static class NaosExtensions
    {
        public static NaosServicesContextOptions AddJobScheduling(
        this NaosServicesContextOptions naosOptions,
        Action<JobSchedulerOptions> optionsAction = null,
        string section = "naos:scheduling")
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));
            EnsureArg.IsNotNull(naosOptions.Context, nameof(naosOptions.Context));

            // needed for mediator
            naosOptions.Context.Services.Scan(scan => scan
                .FromApplicationDependencies()
                .AddClasses(classes => classes.Where(c => c.Name.EndsWith("JobEventHandler")))
                //.FromAssembliesOf(typeof(JobEventHandler<>))
                //.AddClasses()
                .AsImplementedInterfaces());

            naosOptions.Context.Services.AddSingleton<IJobScheduler>(sp =>
            {
                var options = new JobSchedulerOptions(
                    sp.GetRequiredService<ILoggerFactory>(),
                    sp.CreateScope().ServiceProvider.GetService(typeof(IMediator)) as IMediator,
                    new ServiceProviderJobFactory(sp));
                optionsAction?.Invoke(options);

                return new JobScheduler(
                    sp.GetRequiredService<ILoggerFactory>(),
                    new InProcessMutex(sp.GetRequiredService<ILoggerFactory>()),
                    options);
            });

            naosOptions.Context.Services.AddSingleton<IHostedService>(sp =>
                new JobSchedulerHostedService(sp.GetRequiredService<ILoggerFactory>(), sp.GetRequiredService<IJobScheduler>()));

            naosOptions.Context.Messages.Add($"{LogKeys.Startup} naos services builder: job scheduling added"); // TODO: list available commands/handlers
            naosOptions.Context.Services.AddSingleton(new NaosFeatureInformation { Name = "JobScheduling", EchoRoute = "api/echo/jobscheduling" });

            return naosOptions;
        }
    }
}
