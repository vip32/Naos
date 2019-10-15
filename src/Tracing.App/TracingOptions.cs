namespace Naos.Tracing.App
{
    using Microsoft.Extensions.DependencyInjection;

    public class TracingOptions
    {
        public TracingOptions(INaosBuilderContext context)
        {
            this.Context = context;
        }

        public INaosBuilderContext Context { get; }
    }
}
