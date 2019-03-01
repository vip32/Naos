namespace Microsoft.Extensions.DependencyInjection
{
    public class CommandsOptions
    {
        public CommandsOptions(INaosServicesContext context)
        {
            this.Context = context;
        }

        public INaosServicesContext Context { get; }
    }
}
