namespace Naos.Foundation.Domain
{
    using System.ComponentModel;

    public enum RepositoryActionResult
    {
        /// <summary>
        /// Nonde
        /// </summary>
        [Description("no entity action")]
        None,

        /// <summary>
        /// Inserted
        /// </summary>
        [Description("entity inserted")]
        Inserted,

        /// <summary>
        /// Updated
        /// </summary>
        [Description("entity updated")]
        Updated,

        /// <summary>
        /// Deleted
        /// </summary>
        [Description("entity deleted")]
        Deleted
    }
}