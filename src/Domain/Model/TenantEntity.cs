namespace Naos.Core.Domain
{
    using System.Diagnostics;

    /// <summary>
    /// A base entity for all tenant related entities
    /// </summary>
    /// <typeparam name="TId">The type of the identifier.</typeparam>
    /// <seealso cref="Domain.Entity{TId}" />
    [DebuggerDisplay("Id={Id}, Tenant={TenantId}")]
    public abstract class TenantEntity<TId> : Entity<TId>, ITenantEntity
    {
        //[JsonProperty(PropertyName = "_tid")]
        public string TenantId { get; set; }

        public override string ToString() => $"{this.Discriminator} [Id={this.Id}, Tentant={this.TenantId}]";
    }
}
