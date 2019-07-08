namespace Naos.Foundation.Infrastructure.Azure
{
    using System.Diagnostics;

    [DebuggerDisplay("Name={TableName}")]
    public class LogAnalyticsTable
    {
        public string TableName { get; set; }

        public LogAnalyticsColumn[] Columns { get; set; }

        public object[][] Rows { get; set; }
    }
}