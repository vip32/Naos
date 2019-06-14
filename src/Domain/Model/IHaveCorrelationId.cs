namespace Naos.Foundation.Domain
{
    public interface IHaveCorrelationId
    {
        /// <summary>
        /// Gets the correlation id for of the entity.
        /// </summary>
        /// <value>
        /// The correlation id for the entity.
        /// </value>
        string CorrelationId { get; set; }
    }
}
