namespace Naos.Foundation.Domain
{
    using System.Diagnostics;

    /// <summary>
    /// A base entity for all tenant related entities.
    /// </summary>
    /// <typeparam name="TId">The type of the identifier.</typeparam>
    /// <seealso cref="Domain.Entity{TId}" />
    [DebuggerDisplay("Type={GetType().Name}, Id={Id}, Tenant={TenantId}")]
    public abstract class TenantEntity<TId> : Entity<TId>, ITenantEntity
    {
        //[JsonProperty(PropertyName = "_tid")]
        public string TenantId { get; set; }

        public override string ToString() => $"{this.GetType().FullPrettyName()} [Id={this.Id}, Tentant={this.TenantId}]";
    }
}
