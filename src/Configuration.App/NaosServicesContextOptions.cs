namespace Microsoft.Extensions.DependencyInjection
{
    public class NaosServicesContextOptions
    {
        public NaosServicesContextOptions(INaosServicesContext context)
        {
            this.Context = context;
        }

        public INaosServicesContext Context { get; }
    }
}
