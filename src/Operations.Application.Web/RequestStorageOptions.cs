namespace Naos.Operations.Application.Web
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
