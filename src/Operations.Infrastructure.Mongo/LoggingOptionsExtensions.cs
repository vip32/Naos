namespace Microsoft.Extensions.DependencyInjection
{
    using System.Diagnostics.CodeAnalysis;
    using EnsureThat;
    using global::Serilog;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Operations.App;

    [ExcludeFromCodeCoverage]
    public static class LoggingOptionsExtensions
    {
        public static LoggingOptions UseMongo(this LoggingOptions options, LogLevel logLevel = LogLevel.Debug)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            var configuration = options.Context.Configuration?.GetSection("naos:operations:logging:mongo").Get<MongoLoggingConfiguration>();
            if (configuration?.Enabled == true)
            {
                // https://github.com/serilog/serilog-sinks-mongodb
                if (!configuration.CappedMaxDocuments.HasValue && !configuration.CappedMaxSizeMb.HasValue)
                {
                    options.LoggerConfiguration?.WriteTo.MongoDB(
                        configuration.ConnectionString,
                        collectionName: configuration.CollectionName,
                        //database:
                        //outputTemplate: logFileConfiguration.OutputTemplate "{Timestamp:yyyy-MM-dd HH:mm:ss}|{Level} => {CorrelationId} => {Service}::{SourceContext}{NewLine}    {Message}{NewLine}{Exception}",
                        restrictedToMinimumLevel: MapLevel(logLevel));
                }
                else
                {
                    options.LoggerConfiguration?.WriteTo.MongoDBCapped(
                        configuration.ConnectionString,
                        collectionName: configuration.CollectionName,
                        cappedMaxDocuments: configuration.CappedMaxDocuments,
                        cappedMaxSizeMb: configuration.CappedMaxSizeMb ?? 50,
                        //outputTemplate: logFileConfiguration.OutputTemplate "{Timestamp:yyyy-MM-dd HH:mm:ss}|{Level} => {CorrelationId} => {Service}::{SourceContext}{NewLine}    {Message}{NewLine}{Exception}",
                        restrictedToMinimumLevel: MapLevel(logLevel));
                }

                options.Context.Messages.Add($"{LogKeys.Startup} naos services builder: logging mongo sink added (collection={configuration.CollectionName})");
            }

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
