namespace Naos.Core.App.Operations.Serilog
{
    using System;
    using global::Serilog;
    using global::Serilog.Events;
    using Microsoft.Extensions.Logging;
    using SimpleInjector;

    public static class ContainerExtension
    {
        public static Container BuildNaosOperations(this Container container)
        {
            container.Register(ConfigureLogger, Lifestyle.Singleton);
            container.Register(typeof(ILogger<>), typeof(LoggingAdapter<>));

            return container;
        }

        private static ILoggerFactory ConfigureLogger()
        {
            LoggerFactory factory = new LoggerFactory();

            // serilog provider configuration
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Service", AppDomain.CurrentDomain.FriendlyName)
                .WriteTo.Debug()
                .WriteTo.LiterateConsole(/*outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss}|{Level} => {CorrelationId} => {Service}::{SourceContext}{NewLine}    {Message}{NewLine}{Exception}"*/)
                .CreateLogger();

            factory.AddSerilog(Log.Logger);

            return factory;
        }
    }
}
