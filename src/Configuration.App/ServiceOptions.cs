namespace Naos.Core.Configuration.App
{
    using Microsoft.Extensions.DependencyInjection;

    public class ServiceOptions
    {
        public ServiceOptions(INaosBuilder context)
        {
            this.Context = context;
        }

        public INaosBuilder Context { get; }
    }
}
