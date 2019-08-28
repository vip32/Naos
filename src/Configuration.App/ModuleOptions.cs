namespace Microsoft.Extensions.DependencyInjection
{
    public class ModuleOptions
    {
        public ModuleOptions(INaosServicesContext context)
        {
            this.Context = context;
        }

        public INaosServicesContext Context { get; }
    }
}
