namespace Naos.Core.App.Operations.Serilog
{
    using global::Serilog;
    using global::Serilog.Events;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Infrastructure.Azure;
    using Naos.Core.Operations.Infrastructure.Azure.LogAnalytics;
    using SimpleInjector;

    public static class ContainerExtension
    {
        private static IConfiguration internalConfiguration;
        private static LoggerConfiguration internalLoggerConfiguration;
        private static string internalEnvironment;
        private static string internalServiceDescriptor;

        public static Container AddNaosLogging(
            this Container container,
            IConfiguration configuration,
            string environment = "Development",
            string serviceDescriptor = "naos",
            LoggerConfiguration loggerConfiguration = null)
        {
            internalConfiguration = configuration;
            internalLoggerConfiguration = loggerConfiguration;
            internalEnvironment = environment;
            internalServiceDescriptor = serviceDescriptor;
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
            Log.Logger.Information("naos logging initialized");
            return factory;
        }

        private static void ConfigureLogger()
        {
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Environment", internalEnvironment)
                .Enrich.WithProperty("ServiceDescriptor", internalServiceDescriptor)
                .WriteTo.Debug()
                .WriteTo.LiterateConsole(/*outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss}|{Level} => {CorrelationId} => {Service}::{SourceContext}{NewLine}    {Message}{NewLine}{Exception}"*/);
            //.WriteTo.AzureDocumentDB(
            //    uri,
            //    authkey,
            //    dbname,
            //    restrictedToMinimumLevel: LogEventLevel.Debug,
            //    storeTimestampInUtc: true,
            //    collectionName: "LogEvent",
            //    timeToLive: ttl)

            // TODO: split this more so the sinks become better composable
            var logAnalyticsConfiguration = internalConfiguration.GetSection("naos:operations:azureLogAnalytics").Get<LogAnalyticsConfiguration>();
            if (logAnalyticsConfiguration?.Enabled == true
                && logAnalyticsConfiguration?.WorkspaceId.IsNullOrEmpty() == false
                && logAnalyticsConfiguration?.AuthenticationId.IsNullOrEmpty() == false)
            {
                loggerConfiguration.WriteTo.AzureAnalytics(
                    logAnalyticsConfiguration.WorkspaceId,
                    logAnalyticsConfiguration.AuthenticationId, logName: $"LogEvents_{internalEnvironment}" );
            }

            // TODO: application insight setup
            var appInsightsConfiguration = internalConfiguration.GetSection("naos:operations:azureApplicationInsights").Get<ApplicationInsightsConfiguration>();
            if (appInsightsConfiguration?.Enabled == true
                && appInsightsConfiguration?.ApplicationKey.IsNullOrEmpty() == false)
            {
                //loggerConfiguration.WriteTo.AppInsights(appInsightsConfiguration.ApplicationKey);
            }

            Log.Logger = loggerConfiguration.CreateLogger();
        }
    }
}
