namespace Naos.Core.Operations.App.Web
{
    using Microsoft.Extensions.DependencyInjection;

    public class RequestStorageOptions
    {
        public RequestStorageOptions(INaosServicesContext context)
        {
            this.Context = context;
        }

        public INaosServicesContext Context { get; }
    }
}
