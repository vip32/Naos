namespace Naos.Core.JobScheduling.App
{
    using Microsoft.Extensions.DependencyInjection;

    public class JobSchedulingOptions
    {
        public JobSchedulingOptions(INaosServicesContext context)
        {
            this.Context = context;
        }

        public INaosServicesContext Context { get; }
    }
}
