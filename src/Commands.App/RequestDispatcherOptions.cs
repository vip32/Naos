namespace Microsoft.Extensions.DependencyInjection
{
    public class RequestDispatcherOptions
    {
        public RequestDispatcherOptions(INaosBuilderContext context)
        {
            this.Context = context;
        }

        public INaosBuilderContext Context { get; }
    }
}
