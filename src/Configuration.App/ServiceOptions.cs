namespace Naos.Core.Configuration.App
{
    using Microsoft.Extensions.DependencyInjection;

    public class ServiceOptions
    {
        public ServiceOptions(INaosBuilderContext context)
        {
            this.Context = context;
        }

        public INaosBuilderContext Context { get; }
    }
}
