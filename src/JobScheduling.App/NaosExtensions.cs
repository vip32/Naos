namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using EnsureThat;
    using MediatR;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Naos.Configuration.App;
    using Naos.Foundation;
    using Naos.JobScheduling;
    using Naos.JobScheduling.App;
    using Naos.JobScheduling.Domain;
    using Naos.Tracing.Domain;

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
                .FromApplicationDependencies(a => !a.FullName.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase) && !a.FullName.StartsWith("System", StringComparison.OrdinalIgnoreCase))
                .AddClasses(classes => classes.Where(c => c.Name.EndsWith("JobEventHandler", StringComparison.OrdinalIgnoreCase)))
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
                    sp.CreateScope().ServiceProvider.GetService(typeof(ITracer)) as ITracer,
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
