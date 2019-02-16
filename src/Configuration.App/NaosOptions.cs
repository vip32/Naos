namespace Microsoft.Extensions.DependencyInjection
{
    using Microsoft.Extensions.DependencyInjection;

    public class NaosOptions
    {
        public NaosOptions(INaosBuilderContext context)
        {
            this.Context = context;
        }

        public INaosBuilderContext Context { get; }
    }
}
