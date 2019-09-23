namespace Naos.JobScheduling.App
{
    using Microsoft.Extensions.DependencyInjection;

    public class JobSchedulingOptions
    {
        public JobSchedulingOptions(INaosBuilderContext context)
        {
            this.Context = context;
        }

        public INaosBuilderContext Context { get; }
    }
}
