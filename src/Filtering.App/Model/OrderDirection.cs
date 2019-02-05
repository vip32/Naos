namespace Naos.Core.RequestFiltering.App
{
    using System.ComponentModel;

    public enum OrderDirection
    {
        [Description("ascending")]
        Asc = 10,

        [Description("descending")]
        Desc = 20,
    }
}
