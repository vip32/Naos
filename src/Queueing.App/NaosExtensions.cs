namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using EnsureThat;
    //using Microsoft.Extensions.Hosting;
    //using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Configuration.App;
    using Naos.Core.Queueing.App;

    [ExcludeFromCodeCoverage]
    public static class NaosExtensions
    {
        public static NaosServicesContextOptions AddQueueing(
        this NaosServicesContextOptions naosOptions,
        Action<QueueingOptions> optionsAction = null,
        string section = "naos:queueing")
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));
            EnsureArg.IsNotNull(naosOptions.Context, nameof(naosOptions.Context));

            // needed for mediator
            naosOptions.Context.Services.Scan(scan => scan
                .FromApplicationDependencies()
                .AddClasses(classes => classes.Where(c => c.Name.EndsWith("QueueEventHandler")))
                //.FromAssembliesOf(typeof(QueueEventHandler<>))
                //.AddClasses()
                .AsImplementedInterfaces());

            //naosOptions.Context.Services.AddSingleton<IHostedService>(sp =>
            //    new QueueProcessHostedService<T>(sp.GetRequiredService<ILoggerFactory>(), null));

            naosOptions.Context.Messages.Add($"{LogEventKeys.Startup} naos services builder: queueing added"); // TODO: list available commands/handlers
            naosOptions.Context.Services.AddSingleton(new NaosFeatureInformation { Name = "Queueing", EchoRoute = "api/echo/queueing" });

            return naosOptions;
        }
    }
}
