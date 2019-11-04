namespace Naos.Queueing.Application
{
    using Microsoft.Extensions.DependencyInjection;

    public class QueueingOptions
    {
        public QueueingOptions(INaosBuilderContext context)
        {
            this.Context = context;
        }

        public INaosBuilderContext Context { get; }
    }
}
