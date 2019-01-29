namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.IO;
    using EnsureThat;
    using global::Serilog;
    using global::Serilog.Events;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Commands.Operations.Serilog;
    using Naos.Core.Common;
    using Naos.Core.Infrastructure.Azure;
    using Naos.Core.Operations.Infrastructure.Azure.LogAnalytics;

    public static class ServiceExtensions
    {
        private static IConfiguration internalConfiguration;
        private static LoggerConfiguration internalLoggerConfiguration;
        private static string internalEnvironment;
        private static string internalCorrelationId;

        //private static string internalServiceDescriptor;

        public static ServiceConfigurationContext AddOperationsSerilog(
            this ServiceConfigurationContext context,
            string environment = null,
            string correlationId = null,
            //string serviceDescriptor = "naos",
            LoggerConfiguration loggerConfiguration = null)
        {
            EnsureArg.IsNotNull(context, nameof(context));

            internalConfiguration = context.Configuration;
            internalLoggerConfiguration = loggerConfiguration;
            internalEnvironment = environment ?? Environment.GetEnvironmentVariable(EnvironmentKeys.Environment) ?? "Production";
            internalCorrelationId = correlationId;
            //internalServiceDescriptor = serviceDescriptor;

            context.Services.AddSingleton(sp => CreateLoggerFactory());
            context.Services.AddSingleton(typeof(ILogger<>), typeof(LoggingAdapter<>));
            context.Services.AddSingleton(typeof(Logging.ILogger), typeof(LoggingAdapter));

            return context;
        }

        private static ILoggerFactory CreateLoggerFactory()
        {
            if (internalLoggerConfiguration == null)
            {
                ConfigureLogger();
            }
            else
            {
                Log.Logger = internalLoggerConfiguration.CreateLogger();
            }

            var factory = new LoggerFactory();
            factory.AddSerilog(Log.Logger);
            Log.Logger.Debug("{LogKey} logging initialized: serilog", LogEventKeys.Operations);
            return factory;
        }

        private static void ConfigureLogger()
        {
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("HealthChecks.UI", LogEventLevel.Information)
                .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
                .Enrich.With(new ExceptionEnricher())
                .Enrich.WithProperty(LogEventPropertyKeys.Environment, internalEnvironment)
                //.Enrich.WithProperty("ServiceDescriptor", internalServiceDescriptor)
                .Enrich.FromLogContext()
                .WriteTo.Debug()
                .WriteTo.LiterateConsole(
                    restrictedToMinimumLevel: LogEventLevel.Information,
                    //outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {CorrelationId}|{Service}|{SourceContext}: {Message:lj}{NewLine}{Exception}");
                    outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}");

            if (!internalCorrelationId.IsNullOrEmpty())
            {
                loggerConfiguration.Enrich.WithProperty(LogEventPropertyKeys.CorrelationId, internalCorrelationId);
            }

            //.WriteTo.AzureDocumentDB(
            //    uri,
            //    authkey,
            //    dbname,
            //    restrictedToMinimumLevel: LogEventLevel.Debug,
            //    storeTimestampInUtc: true,
            //    collectionName: "LogEvent",
            //    timeToLive: ttl)

            // TODO: split this more so the sinks become better composable
            var logFileConfiguration = internalConfiguration.GetSection("naos:operations:logEvents:file").Get<LogFileConfiguration>();
            if (logFileConfiguration?.Enabled == true)
            {
                var fullFileName = logFileConfiguration.File; // for local web root storage
                if (!logFileConfiguration.Folder.IsNullOrEmpty() && !logFileConfiguration.SubFolder.IsNullOrEmpty())
                {
                    fullFileName = Path.Combine(logFileConfiguration.Folder, "naos_operations", logFileConfiguration.File);
                }
                else if (!logFileConfiguration.Folder.IsNullOrEmpty() && logFileConfiguration.SubFolder.IsNullOrEmpty())
                {
                    fullFileName = Path.Combine(logFileConfiguration.Folder, logFileConfiguration.File);
                }

                // https://github.com/serilog/serilog-aspnetcore
                loggerConfiguration.WriteTo.File(
                    fullFileName,
                    //outputTemplate: diagnosticsLogStreamConfiguration.OutputTemplate "{Timestamp:yyyy-MM-dd HH:mm:ss}|{Level} => {CorrelationId} => {Service}::{SourceContext}{NewLine}    {Message}{NewLine}{Exception}",
                    fileSizeLimitBytes: logFileConfiguration.FileSizeLimitBytes,
                    rollOnFileSizeLimit: logFileConfiguration.RollOnFileSizeLimit,
                    rollingInterval: (RollingInterval)Enum.Parse(typeof(RollingInterval), logFileConfiguration.RollingInterval),
                    shared: logFileConfiguration.Shared,
                    flushToDiskInterval: TimeSpan.FromSeconds(logFileConfiguration.FlushToDiskIntervalSeconds));
            }

            // TODO: split this more so the sinks become better composable
            var diagnosticsLogStreamConfiguration = internalConfiguration.GetSection("naos:operations:logEvents:azureDiagnosticsLogStream").Get<DiagnosticsLogStreamConfiguration>();
            if (diagnosticsLogStreamConfiguration?.Enabled == true)
            {
                // https://github.com/serilog/serilog-aspnetcore
                loggerConfiguration.WriteTo.File(
                    diagnosticsLogStreamConfiguration.File,
                    //outputTemplate: diagnosticsLogStreamConfiguration.OutputTemplate "{Timestamp:yyyy-MM-dd HH:mm:ss}|{Level} => {CorrelationId} => {Service}::{SourceContext}{NewLine}    {Message}{NewLine}{Exception}",
                    fileSizeLimitBytes: diagnosticsLogStreamConfiguration.FileSizeLimitBytes,
                    rollOnFileSizeLimit: diagnosticsLogStreamConfiguration.RollOnFileSizeLimit,
                    shared: diagnosticsLogStreamConfiguration.Shared,
                    flushToDiskInterval: TimeSpan.FromSeconds(diagnosticsLogStreamConfiguration.FlushToDiskIntervalSeconds));
            }

            // TODO: split this more so the sinks become better composable
            var logAnalyticsConfiguration = internalConfiguration.GetSection("naos:operations:azureLogAnalytics").Get<LogAnalyticsConfiguration>(); // TODO: move to operations:logevents:azureLogAnalytics
            if (logAnalyticsConfiguration?.Enabled == true
                && logAnalyticsConfiguration?.WorkspaceId.IsNullOrEmpty() == false
                && logAnalyticsConfiguration?.AuthenticationId.IsNullOrEmpty() == false)
            {
                loggerConfiguration.WriteTo.AzureAnalytics(
                    logAnalyticsConfiguration.WorkspaceId,
                    logAnalyticsConfiguration.AuthenticationId,
                    logName: $"LogEvents_{internalEnvironment}",
                    storeTimestampInUtc: true,
                    logBufferSize: logAnalyticsConfiguration.BufferSize,
                    batchSize: logAnalyticsConfiguration.BatchSize);
            }

            // TODO: application insight setup
            var appInsightsConfiguration = internalConfiguration.GetSection("naos:operations:logEvents:azureApplicationInsights").Get<ApplicationInsightsConfiguration>();
            if (appInsightsConfiguration?.Enabled == true
                && appInsightsConfiguration?.ApplicationKey.IsNullOrEmpty() == false)
            {
                //loggerConfiguration.WriteTo.AppInsights(appInsightsConfiguration.ApplicationKey);
            }

            Log.Logger = loggerConfiguration.CreateLogger();
        }
    }
}
