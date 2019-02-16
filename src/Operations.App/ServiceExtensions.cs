namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using EnsureThat;
    using Naos.Core.Common;
    using Naos.Core.Operations.App;

    public static class ServiceExtensions
    {
        public static NaosOptions AddOperations(
            this NaosOptions naosOptions,
            Action<OperationsOptions> setupAction = null,
            string section = "naos:operations")
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));
            EnsureArg.IsNotNull(naosOptions.Context, nameof(naosOptions.Context));

            setupAction?.Invoke(new OperationsOptions(naosOptions.Context));

            naosOptions.Context.Messages.Add($"{LogEventKeys.General} naos builder: operations added");

            return naosOptions;
        }
    }
}