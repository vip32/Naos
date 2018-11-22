namespace Naos.Core.Operations.Infrastructure.Azure.LogAnalytics.Repositories
{
    using System.Diagnostics;

    [DebuggerDisplay("Name={ColumnName}")]
    public class LogAnalyticsColumn
    {
        public string ColumnName { get; set; }

        public string DataType { get; set; }

        public string ColumnType { get; set; }
    }
}