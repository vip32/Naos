namespace Microsoft.Extensions.DependencyInjection
{
    public class RequestDispatcherOptions
    {
        public RequestDispatcherOptions(INaosServicesContext context)
        {
            this.Context = context;
        }

        public INaosServicesContext Context { get; }
    }
}
