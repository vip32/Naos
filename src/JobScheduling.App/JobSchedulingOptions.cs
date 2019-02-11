namespace Naos.Core.JobScheduling.App
{
    using Microsoft.Extensions.DependencyInjection;

    public class JobSchedulingOptions
    {
        public JobSchedulingOptions(ServiceConfigurationContext context)
        {
            this.Context = context;
        }

        public ServiceConfigurationContext Context { get; }
    }
}
