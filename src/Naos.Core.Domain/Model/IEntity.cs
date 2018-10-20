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

    public interface ITenantEntity : IEntity
    {
        string TenantId { get; set; }
    }
}
