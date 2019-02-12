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

            var configuration = options.Context.Configuration?.GetSection("naos:operations:logEvents:file").Get<LogFileConfiguration>();
            if (configuration?.Enabled == true)
            {
                // configure the serilog sink
                var path = configuration.File.EmptyToNull() ?? $"LogEvents_[PRODUCT]_[CAPABILITY]_[ENVIRONMENT].log"
                    .Replace("[ENVIRONMENT]", options.Environment)
                    .Replace("[PRODUCT]", options.Context.Descriptor?.Product)
                    .Replace("[CAPABILITY]", options.Context.Descriptor?.Capability);
                if (!configuration.Folder.IsNullOrEmpty() && !configuration.SubFolder.IsNullOrEmpty())
                {
                    path = Path.Combine(configuration.Folder, "naos_operations", path);
                }
                else if (!configuration.Folder.IsNullOrEmpty() && configuration.SubFolder.IsNullOrEmpty())
                {
                    path = Path.Combine(configuration.Folder, path);
                }

                // https://github.com/serilog/serilog-aspnetcore
                options.LoggerConfiguration.WriteTo.File(
                    path,
                    //outputTemplate: logFileConfiguration.OutputTemplate "{Timestamp:yyyy-MM-dd HH:mm:ss}|{Level} => {CorrelationId} => {Service}::{SourceContext}{NewLine}    {Message}{NewLine}{Exception}",
                    fileSizeLimitBytes: configuration.FileSizeLimitBytes,
                    rollOnFileSizeLimit: configuration.RollOnFileSizeLimit,
                    rollingInterval: (RollingInterval)Enum.Parse(typeof(RollingInterval), configuration.RollingInterval), // TODO: use tryparse
                    shared: configuration.Shared,
                    flushToDiskInterval: configuration.FlushToDiskIntervalSeconds.HasValue ? TimeSpan.FromSeconds(configuration.FlushToDiskIntervalSeconds.Value) : default(TimeSpan?));

                options.Messages.Add($"{LogEventKeys.Operations} logging: file sink added (path={path})");
            }

            return options;
        }
    }
}
