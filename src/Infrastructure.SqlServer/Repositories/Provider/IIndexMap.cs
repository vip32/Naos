namespace Naos.Foundation.Infrastructure
{
    using System;

    public interface IIndexMap
    {
        string Name { get; set; }
    }

    public interface IIndexMap<T> : IIndexMap
    {
        Func<T, object> Expression { get; set; }
    }
}
