namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.IO;
    using EnsureThat;
    using global::Serilog;
    using Microsoft.Extensions.Configuration;
    using Naos.Core.Commands.Operations.App.Serilog;
    using Naos.Core.Common;
    using Naos.Core.Infrastructure.Azure;
    using Naos.Core.Operations.App;
    using Naos.Core.Operations.Infrastructure.Azure.LogAnalytics;

    public static class LoggingOptionsExtensions
    {
        public static LoggingOptions AddFile(this LoggingOptions options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            var logFileConfiguration = options.Context.Configuration?.GetSection("naos:operations:logEvents:file").Get<LogFileConfiguration>();
            if (logFileConfiguration?.Enabled == true)
            {
                var path = logFileConfiguration.File.EmptyToNull() ?? $"{options.Context.Descriptor.Name.Replace(".", "_")}.log"; // for local web root storage
                if (!logFileConfiguration.Folder.IsNullOrEmpty() && !logFileConfiguration.SubFolder.IsNullOrEmpty())
                {
                    path = Path.Combine(logFileConfiguration.Folder, "naos_operations", path);
                }
                else if (!logFileConfiguration.Folder.IsNullOrEmpty() && logFileConfiguration.SubFolder.IsNullOrEmpty())
                {
                    path = Path.Combine(logFileConfiguration.Folder, path);
                }

                // https://github.com/serilog/serilog-aspnetcore
                options.LoggerConfiguration.WriteTo.File(
                    path,
                    //outputTemplate: logFileConfiguration.OutputTemplate "{Timestamp:yyyy-MM-dd HH:mm:ss}|{Level} => {CorrelationId} => {Service}::{SourceContext}{NewLine}    {Message}{NewLine}{Exception}",
                    fileSizeLimitBytes: logFileConfiguration.FileSizeLimitBytes,
                    rollOnFileSizeLimit: logFileConfiguration.RollOnFileSizeLimit,
                    rollingInterval: (RollingInterval)Enum.Parse(typeof(RollingInterval), logFileConfiguration.RollingInterval), // TODO: use tryparse
                    shared: logFileConfiguration.Shared,
                    flushToDiskInterval: logFileConfiguration.FlushToDiskIntervalSeconds.HasValue ? TimeSpan.FromSeconds(logFileConfiguration.FlushToDiskIntervalSeconds.Value) : default(TimeSpan?));

                options.Messages.Add($"{LogEventKeys.Operations} logging: file sink added (path={path})");
            }

            return options;
        }

        public static LoggingOptions AddDiagnosticsLogStream(this LoggingOptions options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            var diagnosticsLogStreamConfiguration = options.Context.Configuration?.GetSection("naos:operations:logEvents:azureDiagnosticsLogStream").Get<DiagnosticsLogStreamConfiguration>();
            if (diagnosticsLogStreamConfiguration?.Enabled == true)
            {
                // https://github.com/serilog/serilog-aspnetcore
                options.LoggerConfiguration.WriteTo.File(
                    diagnosticsLogStreamConfiguration.File,
                    //outputTemplate: diagnosticsLogStreamConfiguration.OutputTemplate "{Timestamp:yyyy-MM-dd HH:mm:ss}|{Level} => {CorrelationId} => {Service}::{SourceContext}{NewLine}    {Message}{NewLine}{Exception}",
                    fileSizeLimitBytes: diagnosticsLogStreamConfiguration.FileSizeLimitBytes,
                    rollOnFileSizeLimit: diagnosticsLogStreamConfiguration.RollOnFileSizeLimit,
                    rollingInterval: (RollingInterval)Enum.Parse(typeof(RollingInterval), diagnosticsLogStreamConfiguration.RollingInterval), // TODO: use tryparse
                    shared: diagnosticsLogStreamConfiguration.Shared,
                    flushToDiskInterval: diagnosticsLogStreamConfiguration.FlushToDiskIntervalSeconds.HasValue ? TimeSpan.FromSeconds(diagnosticsLogStreamConfiguration.FlushToDiskIntervalSeconds.Value) : default(TimeSpan?));

                options.Messages.Add($"{LogEventKeys.Operations} logging: diagnosticslogstream sink added (path={diagnosticsLogStreamConfiguration.File})");
            }

            return options;
        }

        public static LoggingOptions AddAzureLogAnalytics(this LoggingOptions options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            var logAnalyticsConfiguration = options.Context.Configuration?.GetSection("naos:operations:azureLogAnalytics").Get<LogAnalyticsConfiguration>(); // TODO: move to operations:logevents:azureLogAnalytics
            if (logAnalyticsConfiguration?.Enabled == true
                && logAnalyticsConfiguration?.WorkspaceId.IsNullOrEmpty() == false
                && logAnalyticsConfiguration?.AuthenticationId.IsNullOrEmpty() == false)
            {
                var logName = $"LogEvents_{options.Environment}";
                options.LoggerConfiguration.WriteTo.AzureAnalytics(
                    logAnalyticsConfiguration.WorkspaceId,
                    logAnalyticsConfiguration.AuthenticationId,
                    logName: logName,
                    storeTimestampInUtc: true,
                    logBufferSize: logAnalyticsConfiguration.BufferSize,
                    batchSize: logAnalyticsConfiguration.BatchSize);

                options.Messages.Add($"{LogEventKeys.Operations} logging: azureloganalytics sink added (name={logName}, workspace={logAnalyticsConfiguration.WorkspaceId})");
            }

            return options;
        }

        public static LoggingOptions AddAzureApplicationInsights(this LoggingOptions options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            var appInsightsConfiguration = options.Context.Configuration?.GetSection("naos:operations:logEvents:azureApplicationInsights").Get<ApplicationInsightsConfiguration>();
            if (appInsightsConfiguration?.Enabled == true
                && appInsightsConfiguration?.ApplicationKey.IsNullOrEmpty() == false)
            {
                //options.LoggerConfiguration.WriteTo.AppInsights(appInsightsConfiguration.ApplicationKey);

                options.Messages.Add($"{LogEventKeys.Operations} logging: azureapplicationinsightssink added (application={appInsightsConfiguration.ApplicationKey})");
            }

            return options;
        }
    }
}
