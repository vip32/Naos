namespace Microsoft.Extensions.DependencyInjection
{
    public class CommandRequestOptions
    {
        public CommandRequestOptions(INaosBuilderContext context)
        {
            this.Context = context;
        }

        public INaosBuilderContext Context { get; }
    }
}
