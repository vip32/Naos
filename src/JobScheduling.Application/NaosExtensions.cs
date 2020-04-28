namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using EnsureThat;
    using MediatR;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Naos.Configuration.Application;
    using Naos.Foundation;
    using Naos.JobScheduling;
    using Naos.JobScheduling.Application;
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
                .FromApplicationDependencies(a => !a.FullName.StartsWithAny(new[] { "Microsoft", "System", "Scrutor", "Consul" }))
                .AddClasses(classes => classes.Where(c => c.Name.EndsWith("JobEventHandler", StringComparison.OrdinalIgnoreCase)))
                //.FromAssembliesOf(typeof(JobEventHandler<>))
                //.AddClasses()
                .AsImplementedInterfaces());

            naosOptions.Context.Services.AddSingleton<IJobScheduler>(sp =>
            {
                var options = new JobSchedulerOptions(
                    sp.GetRequiredService<ILoggerFactory>(),
                    sp.CreateScope().ServiceProvider.GetService(typeof(IMediator)) as IMediator,
                    new ServiceProviderJobFactory(sp.CreateScope().ServiceProvider));
                optionsAction?.Invoke(options);

                return new JobScheduler(
                    sp.GetRequiredService<ILoggerFactory>(),
                    sp.CreateScope().ServiceProvider.GetService(typeof(ITracer)) as ITracer,
                    new InProcessMutex(sp.GetRequiredService<ILoggerFactory>()),
                    options);
            });

            naosOptions.Context.Services.AddSingleton<IHostedService>(sp =>
                new JobSchedulerHostedService(sp.GetRequiredService<ILoggerFactory>(), sp.GetRequiredService<IJobScheduler>()));

            naosOptions.Context.Messages.Add("naos services builder: job scheduling added"); // TODO: list available commands/handlers
            naosOptions.Context.Services.AddSingleton(new NaosFeatureInformation { Name = "JobScheduling", EchoRoute = "naos/jobscheduling/echo" });

            return naosOptions;
        }
    }
}
