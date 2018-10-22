namespace Naos.Core.Domain
{
    public interface IEntity
    {
        object Id { get; }

        string EntityType { get; }
    }

    public interface IEntity<TId> : IEntity
    {
        new TId Id { get; set; }
    }
}
