namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.IO;
    using EnsureThat;
    using global::Serilog;
    using Microsoft.Extensions.Configuration;
    using Naos.Core.Common;
    using Naos.Core.Infrastructure.Azure;
    using Naos.Core.Operations.App;

    public static class LoggingOptionsExtensions
    {
        public static LoggingOptions AddFile(this LoggingOptions options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            var logFileConfiguration = options.Context.Configuration?.GetSection("naos:operations:logEvents:file").Get<LogFileConfiguration>();
            if (logFileConfiguration?.Enabled == true)
            {
                // configure the serilog sink
                var path = logFileConfiguration.File.EmptyToNull() ?? $"LogEvents_[PRODUCT]_[CAPABILITY]_[ENVIRONMENT].log"
                    .Replace("[ENVIRONMENT]", options.Environment)
                    .Replace("[PRODUCT]", options.Context.Descriptor?.Product)
                    .Replace("[CAPABILITY]", options.Context.Descriptor?.Capability);
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
                // configure the serilog sink
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
    }
}
