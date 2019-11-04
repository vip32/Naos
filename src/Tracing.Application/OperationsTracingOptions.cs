namespace Naos.Tracing.Application
{
    using Microsoft.Extensions.DependencyInjection;

    public class OperationsTracingOptions
    {
        public OperationsTracingOptions(INaosBuilderContext context)
        {
            this.Context = context;
        }

        public INaosBuilderContext Context { get; }
    }
}
