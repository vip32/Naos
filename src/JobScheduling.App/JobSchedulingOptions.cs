namespace Naos.Core.JobScheduling.App
{
    using Microsoft.Extensions.DependencyInjection;

    public class JobSchedulingOptions
    {
        public JobSchedulingOptions(INaosBuilder context)
        {
            this.Context = context;
        }

        public INaosBuilder Context { get; }
    }
}
