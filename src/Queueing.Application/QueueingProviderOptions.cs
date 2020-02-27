namespace Naos.Queueing.Application
{
    using Microsoft.Extensions.DependencyInjection;

    public class QueueingProviderOptions<TData>
        where TData : class
    {
        public QueueingProviderOptions(INaosBuilderContext context)
        {
            this.Context = context;
        }

        public INaosBuilderContext Context { get; }
    }
}
