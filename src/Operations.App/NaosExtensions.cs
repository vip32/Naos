namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using EnsureThat;
    using Naos.Core.Common;
    using Naos.Core.Operations.App;

    [ExcludeFromCodeCoverage]
    public static class NaosExtensions
    {
        public static NaosOptions AddOperations(
            this NaosOptions naosOptions,
            Action<OperationsOptions> setupAction = null,
            string section = "naos:operations")
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));
            EnsureArg.IsNotNull(naosOptions.Context, nameof(naosOptions.Context));

            setupAction?.Invoke(new OperationsOptions(naosOptions.Context));

            naosOptions.Context.Messages.Add($"{LogEventKeys.Startup} naos builder: operations added");

            return naosOptions;
        }
    }
}