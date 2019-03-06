namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using EnsureThat;
    using Naos.Core.Common;
    using Naos.Core.Configuration.App;
    using Naos.Core.Operations.App;
    using Naos.Core.Operations.Domain;

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

            naosOptions.Context.Messages.Add($"{LogEventKeys.Startup} naos services builder: operations added");
            naosOptions.Context.Services.AddSingleton(new NaosFeatureInformation { Name = "Operations" });

            return naosOptions;
        }
    }
}