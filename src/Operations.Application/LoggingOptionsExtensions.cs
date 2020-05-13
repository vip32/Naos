namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using EnsureThat;
    using global::Serilog;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Operations;
    using Naos.Operations.Application;
    using Serilog.Configuration;

    [ExcludeFromCodeCoverage]
    public static class LoggingOptionsExtensions
    {
        public static LoggingOptions UseFile(this LoggingOptions options, LogLevel logLevel = LogLevel.Debug)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            var configuration = options.Context.Configuration?.GetSection("naos:operations:logging:file").Get<FileLoggingConfiguration>();
            if (configuration?.Enabled == true)
            {
                // configure the serilog sink
                var path = configuration.File.EmptyToNull() ?? "logevents_{environment}_{product}_{capability}.log"
                    .Replace("{environment}", options.Context.Environment.ToLower())
                    .Replace("{product}", options.Context.Descriptor?.Product?.ToLower())
                    .Replace("{capability}", options.Context.Descriptor?.Capability?.ToLower());

                if (!configuration.Folder.IsNullOrEmpty() && !configuration.SubFolder.IsNullOrEmpty())
                {
                    path = Path.Combine(configuration.Folder, "naos_operations", path);
                }
                else if (!configuration.Folder.IsNullOrEmpty() && configuration.SubFolder.IsNullOrEmpty())
                {
                    path = Path.Combine(configuration.Folder, path);
                }

                // https://github.com/serilog/serilog-aspnetcore
                options.LoggerConfiguration?.WriteTo.File(
                    path,
                    //outputTemplate: logFileConfiguration.OutputTemplate "{Timestamp:yyyy-MM-dd HH:mm:ss}|{Level} => {CorrelationId} => {Service}::{SourceContext}{NewLine}    {Message}{NewLine}{Exception}",
                    restrictedToMinimumLevel: MapLevel(logLevel),
                    fileSizeLimitBytes: configuration.FileSizeLimitBytes,
                    rollOnFileSizeLimit: configuration.RollOnFileSizeLimit,
                    rollingInterval: (RollingInterval)Enum.Parse(typeof(RollingInterval), configuration.RollingInterval), // TODO: use tryparse
                    shared: configuration.Shared,
                    flushToDiskInterval: configuration.FlushToDiskIntervalSeconds.HasValue ? TimeSpan.FromSeconds(configuration.FlushToDiskIntervalSeconds.Value) : default(TimeSpan?));

                options.Context.Messages.Add($"naos services builder: logging file sink added (path={path})");
            }

            return options;
        }

        public static LoggingOptions UseConsole(this LoggingOptions options, LogLevel logLevel = LogLevel.Information)
        {
            // don't use in production, console logging is blockinghttps://medium.com/asos-techblog/maximising-net-core-api-performance-11ad883436c
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            var configuration = options.Context.Configuration?.GetSection("naos:operations:logging:console").Get<ConsoleLoggingConfiguration>();
            if (configuration?.Enabled == true)
            {
                // configure the serilog sink
                options.LoggerConfiguration?.WriteTo.LiterateConsole(
                    restrictedToMinimumLevel: MapLevel(logLevel),
                    //outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {CorrelationId}|{Service}|{SourceContext}: {Message:lj}{NewLine}{Exception}");
                    outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}");

                options.Context.Messages.Add("naos services builder: logging console sink added");
            }

            return options;
        }

        public static LoggingOptions UseSeq(this LoggingOptions options, LogLevel logLevel = LogLevel.Information)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            var configuration = options.Context.Configuration?.GetSection("naos:operations:logging:seq").Get<SeqLoggingConfiguration>();
            if (configuration?.Enabled == true && !configuration.Endpoint.IsNullOrEmpty())
            {
                // configure the serilog sink
                options.LoggerConfiguration?.WriteTo.Seq(
                    configuration.Endpoint,
                    restrictedToMinimumLevel: MapLevel(logLevel));

                options.Context.Messages.Add($"naos services builder: logging seq sink added (endpoint={configuration.Endpoint})");
            }

            return options;
        }

        public static LoggingOptions UseSink(this LoggingOptions options, Func<LoggerSinkConfiguration, LoggerConfiguration> action)
        {
            action?.Invoke(options.LoggerConfiguration.WriteTo);

            return options;
        }

        public static Serilog.Events.LogEventLevel MapLevel(LogLevel logLevel) // TODO: make generally available
        {
            if (logLevel == LogLevel.Trace)
            {
                return Serilog.Events.LogEventLevel.Verbose;
            }
            else if (logLevel == LogLevel.Debug)
            {
                return Serilog.Events.LogEventLevel.Debug;
            }
            else if (logLevel == LogLevel.Error)
            {
                return Serilog.Events.LogEventLevel.Error;
            }
            else if (logLevel == LogLevel.Critical)
            {
                return Serilog.Events.LogEventLevel.Fatal;
            }
            else if (logLevel == LogLevel.Warning)
            {
                return Serilog.Events.LogEventLevel.Warning;
            }

            return Serilog.Events.LogEventLevel.Information;
        }
    }
}
