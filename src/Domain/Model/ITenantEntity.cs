namespace Naos.Core.Domain
{
    public interface ITenantEntity : IEntity
    {
        string TenantId { get; set; }
    }
}
