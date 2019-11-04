namespace Microsoft.Extensions.DependencyInjection
{
    public class CommandsOptions
    {
        public CommandsOptions(INaosBuilderContext context)
        {
            this.Context = context;
        }

        public INaosBuilderContext Context { get; }
    }
}
