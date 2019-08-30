namespace Microsoft.Extensions.DependencyInjection
{
    public class RequestCommandOptions
    {
        public RequestCommandOptions(INaosBuilderContext context)
        {
            this.Context = context;
        }

        public INaosBuilderContext Context { get; }
    }
}
