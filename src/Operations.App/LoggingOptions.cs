namespace Naos.Core.Operations.App
{
    using System.Collections.Generic;
    using Microsoft.Extensions.DependencyInjection;
    using Serilog;

    public class LoggingOptions
    {
        public LoggingOptions(INaosBuilderContext context, LoggerConfiguration loggerConfiguration, string environment)
        {
            this.Context = context;
            this.LoggerConfiguration = loggerConfiguration;
            this.Environment = environment;
        }

        public INaosBuilderContext Context { get; }

        public LoggerConfiguration LoggerConfiguration { get; }

        public string Environment { get; set; }

        public List<string> Messages { get; set; } = new List<string>();
    }
}
