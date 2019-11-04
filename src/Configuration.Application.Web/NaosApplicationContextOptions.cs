namespace Microsoft.Extensions.DependencyInjection
{
    public class NaosApplicationContextOptions
    {
        public NaosApplicationContextOptions(INaosApplicationContext context)
        {
            this.Context = context;
        }

        public INaosApplicationContext Context { get; }
    }
}
