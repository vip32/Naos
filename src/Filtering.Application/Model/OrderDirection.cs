namespace Naos.RequestFiltering.Application
{
    using System.ComponentModel;

    public enum OrderDirection
    {
        /// <summary>
        /// Ascending
        /// </summary>
        [Description("ascending")]
        Asc = 10,

        /// <summary>
        /// Descending
        /// </summary>
        [Description("descending")]
        Desc = 20,
    }
}
