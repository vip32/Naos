namespace Naos.Core.Domain
{
    /// <summary>
    /// An aggregate root is a class which works as an entry point to our aggregate.
    /// All business operations should go through the root. This way, the aggregate root
    /// can take care of keeping the aggregate in a consistent state.
    /// </summary>
    public interface IAggregateRoot
    {
    }
}
