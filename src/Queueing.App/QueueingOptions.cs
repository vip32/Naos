namespace Naos.Core.Queueing.App
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
