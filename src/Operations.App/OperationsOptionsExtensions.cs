namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using EnsureThat;
    using global::Serilog;
    using global::Serilog.Events;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Common.Web;
    using Naos.Core.Operations.App;

    public static class OperationsOptionsExtensions
    {
        private static string internalCorrelationId;
        private static ILoggerFactory factory;

        public static OperationsOptions AddLogging(
            this OperationsOptions options,
            Action<LoggingOptions> setupAction = null,
            string environment = null,
            string correlationId = null,
            LoggerConfiguration loggerConfiguration = null)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Messages.Add($"{LogEventKeys.Startup} naos services builder: logging added");
            internalCorrelationId = correlationId;

            var loggingOptions = new LoggingOptions(
                options.Context,
                loggerConfiguration ?? new LoggerConfiguration());

            InitializeLogger(loggingOptions);
            setupAction?.Invoke(loggingOptions);

            options.Context.Services.AddSingleton(sp => CreateLoggerFactory(loggingOptions));
            options.Context.Services.AddSingleton(typeof(ILogger<>), typeof(LoggingAdapter<>));
            options.Context.Services.AddSingleton(typeof(Logging.ILogger), typeof(LoggingAdapter));

            options.Context.Services.AddTransient<HttpClientLogHandler>();
            options.Context.Services.Replace(ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, HttpClientLogHandlerBuilderFilter>());

            return options;
        }

        private static ILoggerFactory CreateLoggerFactory(LoggingOptions loggingOptions)
        {
            if (factory == null) // extra singleton because sometimes this is called multiple times. serilog does not like that
            {
                    Log.Logger = loggingOptions.LoggerConfiguration.CreateLogger();

                factory = new LoggerFactory();
                factory.AddSerilog(Log.Logger);
                Log.Logger.Debug("{LogKey:l} naos services builder: logging initialized (type=Serilog)", LogEventKeys.Startup);
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
                .WriteTo.LiterateConsole(
                    restrictedToMinimumLevel: LogEventLevel.Information,
                    //outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {CorrelationId}|{Service}|{SourceContext}: {Message:lj}{NewLine}{Exception}");
                    outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}")
#endif
                .Enrich.With(new ExceptionEnricher())
                .Enrich.With(new TicksEnricher())
                .Enrich.WithProperty(LogEventPropertyKeys.Environment, loggingOptions.Context.Environment)
                //.Enrich.WithProperty("ServiceDescriptor", internalServiceDescriptor)
                .Enrich.FromLogContext();

            if (!internalCorrelationId.IsNullOrEmpty())
            {
                loggingOptions.LoggerConfiguration.Enrich.WithProperty(LogEventPropertyKeys.CorrelationId, internalCorrelationId);
            }
        }
    }
}
