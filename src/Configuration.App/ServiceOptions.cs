namespace Naos.Core.Configuration.App
{
    using Microsoft.Extensions.DependencyInjection;

    public class ServiceOptions
    {
        public ServiceOptions(ServiceConfigurationContext context)
        {
            this.Context = context;
        }

        public ServiceConfigurationContext Context { get; }
    }
}
