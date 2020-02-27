namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using EnsureThat;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Naos.Configuration.Application;
    using Naos.Queueing;
    //using Microsoft.Extensions.Hosting;
    using Naos.Queueing.Application;
    using Naos.Queueing.Domain;
    using Naos.Tracing.Domain;

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
                .FromApplicationDependencies(a => !a.FullName.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase) && !a.FullName.StartsWith("System", StringComparison.OrdinalIgnoreCase))
                .AddClasses(classes => classes.Where(c => c.Name.EndsWith("QueueEventHandler", StringComparison.OrdinalIgnoreCase)))
                //.FromAssembliesOf(typeof(QueueEventHandler<>))
                //.AddClasses()
                .AsImplementedInterfaces());

            //naosOptions.Context.Services.AddSingleton<IHostedService>(sp =>
            //    new QueueProcessHostedService<T>(sp.GetRequiredService<ILoggerFactory>(), null));

            naosOptions.Context.Messages.Add($"{LogKeys.Startup} naos services builder: queueing added"); // TODO: list available commands/handlers
            naosOptions.Context.Services.AddSingleton(new NaosFeatureInformation { Name = "Queueing", EchoRoute = "naos/queueing/echo" });

            return naosOptions;
        }

        [ExcludeFromCodeCoverage]
        public static QueueingOptions UseInMemoryQueue<TData>(
                this QueueingOptions options,
                Action<QueueingProviderOptions<TData>> optionsAction = null,
                string section = "naos:queueing:serviceBus")
                where TData : class
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<IQueue<TData>>(sp =>
            {
                return new InMemoryQueue<TData>(o => o
                    .Mediator(sp.GetService<IMediator>())
                    .Tracer(sp.GetService<ITracer>())
                    .LoggerFactory(sp.GetService<ILoggerFactory>())
                    .NoRetries());
            });

            //optionsAction?.Invoke(broker);

            //options.Context.Services.AddHealthChecks()
            //    .AddAzureServiceBusTopic(configuration.ConnectionString, configuration.EntityPath, "messaging-broker-servicebus");

            options.Context.Messages.Add($"{LogKeys.Startup} naos services builder: queueing added (provider={nameof(InMemoryQueue<TData>)})");

            return options;
        }
    }
}
