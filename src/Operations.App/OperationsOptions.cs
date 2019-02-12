namespace Naos.Core.Operations.App
{
    using Microsoft.Extensions.DependencyInjection;

    public class OperationsOptions
    {
        public OperationsOptions(INaosBuilder context)
        {
            this.Context = context;
        }

        public INaosBuilder Context { get; }
    }
}
