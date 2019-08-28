namespace Microsoft.Extensions.DependencyInjection
{
    public class NaosServicesContextOptions
    {
        public NaosServicesContextOptions(INaosBuilderContext context)
        {
            this.Context = context;
        }

        public INaosBuilderContext Context { get; }
    }
}
