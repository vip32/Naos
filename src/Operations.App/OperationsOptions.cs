namespace Naos.Core.Operations.App
{
    using Microsoft.Extensions.DependencyInjection;

    public class OperationsOptions
    {
        public OperationsOptions(INaosServicesContext context)
        {
            this.Context = context;
        }

        public INaosServicesContext Context { get; }
    }
}
