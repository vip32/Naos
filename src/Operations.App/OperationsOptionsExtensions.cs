namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using EnsureThat;
    using global::Serilog;
    using global::Serilog.Events;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Operations.App;
    using Naos.Core.Tracing.Domain;
    using Naos.Foundation;
    using Naos.Foundation.Application;

    [ExcludeFromCodeCoverage]
    public static class OperationsOptionsExtensions
    {
        private static string internalCorrelationId;
        private static ILoggerFactory factory;

        public static OperationsOptions AddLogging(
        this OperationsOptions options,
        Action<LoggingOptions> optionsAction = null,
        string environment = null,
        string correlationId = null, // needed when testing (static correlationid)
        LoggerConfiguration loggerConfiguration = null)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Messages.Add($"{LogKeys.Startup} naos services builder: logging added");
            internalCorrelationId = correlationId;

            var loggingOptions = new LoggingOptions(
                options.Context,
                loggerConfiguration ?? new LoggerConfiguration());

            InitializeLogger(loggingOptions);
            optionsAction?.Invoke(loggingOptions);

            options.Context.Services.AddSingleton(sp => CreateLoggerFactory(loggingOptions));
            options.Context.Services.AddSingleton(typeof(ILogger<>), typeof(LoggingAdapter<>));
            options.Context.Services.AddSingleton(typeof(Logging.ILogger), typeof(LoggingAdapter));

            options.Context.Services.AddScoped<HttpClientLogHandler>();
            options.Context.Services.AddScoped<HttpClientTracerHandler>();
            options.Context.Services.Replace(ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, HttpClientLogHandlerBuilderFilter>()); // scoped?

            return options;
        }

        public static OperationsOptions AddInteractiveConsole(
            this OperationsOptions options,
            bool enabled = true)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            if (options.Context.IsConsoleEnabled() && enabled)
            {
                Console2.WriteTextLogo();

                // needed for mediator, register console commands + handlers
                options.Context.Services.Scan(scan => scan
                    .FromApplicationDependencies(a => !a.FullName.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase) && !a.FullName.StartsWith("System", StringComparison.OrdinalIgnoreCase))
                    .AddClasses(classes => classes.Where(c => c.Name.EndsWith("ConsoleCommand", StringComparison.OrdinalIgnoreCase) || c.Name.EndsWith("ConsoleCommandEventHandler", StringComparison.OrdinalIgnoreCase)))
                    .AsImplementedInterfaces());

                options.Context.Services.AddSingleton<Hosting.IHostedService>(sp =>
                    {
                        return new InteractiveConsoleHostedService(
                          sp.GetRequiredService<ILoggerFactory>(),
                          (IMediator)sp.CreateScope().ServiceProvider.GetService(typeof(IMediator)),
                          sp.GetServices<IConsoleCommand>());
                    });
            }

            return options;
        }

        public static OperationsOptions AddTracing(
            this OperationsOptions options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Messages.Add($"{LogKeys.Startup} naos services builder: tracing added");
            options.Context.Services.AddScoped<ITracer>(sp =>
            {
                return new Tracer(
                    new AsyncLocalScopeManager((IMediator)sp.CreateScope().ServiceProvider.GetService(typeof(IMediator))),
                    sp.GetService<ISampler>());
            });
            options.Context.Services.AddSingleton<ISampler, ConstantSampler>(); // TODO: configure different samplers

            return options;
        }

        private static ILoggerFactory CreateLoggerFactory(LoggingOptions loggingOptions)
        {
            if (factory == null) // extra singleton because sometimes this is called multiple times. serilog does not like that
            {
                Log.Logger = loggingOptions.LoggerConfiguration.CreateLogger();

                factory = new LoggerFactory();
                factory.AddSerilog(Log.Logger);
                Log.Logger.Debug("{LogKey:l} naos services builder: logging initialized (type=Serilog)", LogKeys.Startup);
            }

            return factory;
        }

        private static void InitializeLogger(LoggingOptions loggingOptions)
        {
            loggingOptions.LoggerConfiguration
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Information)
                .MinimumLevel.Override("HealthChecks.UI", LogEventLevel.Information)
                .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
#if DEBUG
                .WriteTo.Debug()
#endif
                //.Enrich.With<EventTypeEnricher>() // https://nblumhardt.com/2015/10/assigning-event-types-to-serilog-events/
                .Enrich.With(new IdEnricher())
                .Enrich.With(new ExceptionEnricher())
                .Enrich.With(new TicksEnricher())
                .Enrich.WithProperty(LogPropertyKeys.Environment, loggingOptions.Context.Environment)
                .Enrich.WithProperty(LogPropertyKeys.ServiceName, loggingOptions.Context.Descriptor.Name)
                .Enrich.WithProperty(LogPropertyKeys.ServiceProduct, loggingOptions.Context.Descriptor.Product)
                .Enrich.WithProperty(LogPropertyKeys.ServiceCapability, loggingOptions.Context.Descriptor.Capability)
                .Enrich.FromLogContext();

            if (!internalCorrelationId.IsNullOrEmpty())
            {
                loggingOptions.LoggerConfiguration.Enrich.WithProperty(LogPropertyKeys.CorrelationId, internalCorrelationId);
            }
        }
    }
}
