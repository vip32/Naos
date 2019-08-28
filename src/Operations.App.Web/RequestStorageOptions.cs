namespace Naos.Core.Operations.App.Web
{
    using Microsoft.Extensions.DependencyInjection;

    public class RequestStorageOptions
    {
        public RequestStorageOptions(INaosBuilderContext context)
        {
            this.Context = context;
        }

        public INaosBuilderContext Context { get; }
    }
}
