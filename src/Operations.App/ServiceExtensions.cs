namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using Naos.Core.Operations.App;

    public static class ServiceExtensions
    {
        public static INaosBuilderContext AddOperations(
            this INaosBuilderContext context,
            Action<OperationsOptions> setupAction = null,
            string section = "naos:operations")
        {
            setupAction?.Invoke(new OperationsOptions(context));

            return context;
        }
    }
}