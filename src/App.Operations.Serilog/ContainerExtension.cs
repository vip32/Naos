namespace Naos.Core.App.Operations.Serilog
{
    using System;
    using global::Serilog;
    using global::Serilog.Events;
    using Microsoft.Extensions.Logging;
    using SimpleInjector;

    public static class ContainerExtension
    {
        private static LoggerConfiguration internalLoggerConfiguration;

        public static Container AddNaosLogging(this Container container, LoggerConfiguration loggerConfiguration = null)
        {
            internalLoggerConfiguration = loggerConfiguration;
            container.Register(CreateLoggerFactory, Lifestyle.Singleton);
            container.Register(typeof(ILogger<>), typeof(LoggingAdapter<>));

            return container;
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
            return factory;
        }

        private static void ConfigureLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Service", AppDomain.CurrentDomain.FriendlyName)
                .WriteTo.Debug()
                .WriteTo.LiterateConsole(/*outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss}|{Level} => {CorrelationId} => {Service}::{SourceContext}{NewLine}    {Message}{NewLine}{Exception}"*/)
                //.WriteTo.AzureDocumentDB(
                //    uri,
                //    authkey,
                //    dbname,
                //    restrictedToMinimumLevel: LogEventLevel.Debug,
                //    storeTimestampInUtc: true,
                //    collectionName: "LogEvent",
                //    timeToLive: ttl)
                .CreateLogger();
        }
    }
}
