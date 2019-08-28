namespace Microsoft.Extensions.DependencyInjection
{
    public class ModuleOptions
    {
        public ModuleOptions(INaosBuilderContext context)
        {
            this.Context = context;
        }

        public INaosBuilderContext Context { get; }
    }
}
