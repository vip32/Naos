namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Configuration.App;
    using Naos.Operations.App;

    [ExcludeFromCodeCoverage]
    public static class NaosExtensions
    {
        public static NaosServicesContextOptions AddOperations(
            this NaosServicesContextOptions naosOptions,
            Action<OperationsOptions> optionsAction = null,
            string section = "naos:operations")
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));
            EnsureArg.IsNotNull(naosOptions.Context, nameof(naosOptions.Context));

            optionsAction?.Invoke(new OperationsOptions(naosOptions.Context));

            naosOptions.Context.Services.AddScoped<ILogEventService, LogEventService>();

            naosOptions.Context.Messages.Add($"{LogKeys.Startup} naos services builder: operations added");
            naosOptions.Context.Services.AddSingleton(new NaosFeatureInformation { Name = "Operations" });

            return naosOptions;
        }
    }
}