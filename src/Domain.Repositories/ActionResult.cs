namespace Naos.Core.Domain.Repositories
{
    using System.ComponentModel;

    public enum ActionResult
    {
        [Description("no entity action")]
        None,

        [Description("entity inserted")]
        Inserted,

        [Description("entity updated")]
        Updated,

        [Description("entity deleted")]
        Deleted
    }
}