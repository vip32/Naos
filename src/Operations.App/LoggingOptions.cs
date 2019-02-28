namespace Naos.Core.Operations.App
{
    using Microsoft.Extensions.DependencyInjection;
    using Serilog;

    public class LoggingOptions
    {
        public LoggingOptions(
            INaosServicesContext context,
            LoggerConfiguration loggerConfiguration)
        {
            this.Context = context;
            this.LoggerConfiguration = loggerConfiguration;
        }

        public INaosServicesContext Context { get; }

        public LoggerConfiguration LoggerConfiguration { get; }
    }
}
