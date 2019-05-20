namespace Naos.Core.Common
{
    using System;

    public class Mapper<TSource, TDestination> : IMapper<TSource, TDestination> // TODO: rename to ActionMapper?
    {
        private readonly Action<TSource, TDestination> action;

        public Mapper(Action<TSource, TDestination> action)
        {
            this.action = action;
        }

        public void Map(TSource source, TDestination destination)
        {
            if(source != null && destination != null)
            {
                this.action(source, destination);
            }
        }
    }
}
