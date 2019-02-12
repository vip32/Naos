namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using Naos.Core.Common;
    using Naos.Core.Operations.App;

    public static class ServiceExtensions
    {
        public static INaosBuilderContext AddOperations(
            this INaosBuilderContext context,
            Action<OperationsOptions> setupAction = null,
            string section = "naos:operations")
        {
            setupAction?.Invoke(new OperationsOptions(context));

            context.Messages.Add($"{LogEventKeys.General} naos builder: operations added");

            return context;
        }
    }
}