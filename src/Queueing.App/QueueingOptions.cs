namespace Naos.Core.Queueing.App
{
    using Microsoft.Extensions.DependencyInjection;

    public class QueueingOptions
    {
        public QueueingOptions(INaosServicesContext context)
        {
            this.Context = context;
        }

        public INaosServicesContext Context { get; }
    }
}
