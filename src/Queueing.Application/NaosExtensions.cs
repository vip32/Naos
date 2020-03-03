namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using EnsureThat;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Naos.Configuration.Application;
    using Naos.Foundation;
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

            optionsAction?.Invoke(new QueueingOptions(naosOptions.Context));

            naosOptions.Context.Messages.Add($"{LogKeys.Startup} naos services builder: queueing added"); // TODO: list available commands/handlers
            naosOptions.Context.Services.AddSingleton(new NaosFeatureInformation { Name = "Queueing", EchoRoute = "naos/queueing/echo" });

            return naosOptions;
        }

        [ExcludeFromCodeCoverage]
        public static QueueingOptions UseInMemoryQueue<TData>(
                this QueueingOptions options,
                Action<QueueingProviderOptions<TData>> optionsAction = null,
                int? retries = null,
                string section = "naos:queueing:serviceBus")
                where TData : class
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            var queueName = typeof(TData).PrettyName().ToLower();
            options.Context.Services.AddSingleton<IQueue<TData>>(sp =>
            {
                return new InMemoryQueue<TData>(o => o
                    .Mediator(sp.GetService<IMediator>())
                    .Tracer(sp.GetService<ITracer>())
                    .LoggerFactory(sp.GetService<ILoggerFactory>())
                    .QueueName(queueName)
                    .Retries(retries));
            });

            optionsAction?.Invoke(
                new QueueingProviderOptions<TData>(options.Context));

            options.Context.Messages.Add($"{LogKeys.Startup} naos services builder: queueing provider added (provider={nameof(InMemoryQueue<TData>)}, queue={queueName})");

            return options;
        }
    }
}
