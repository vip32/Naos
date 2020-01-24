namespace Naos.Operations.Application
{
    using System;
    using System.Collections.Generic;

    public class NaosHealthReport
    {
        public string Status { get; set; }

        public TimeSpan Duration { get; set; }

        public string Took { get; set; }

        public string Timestamp { get; set; }

        public IDictionary<string, NaosHealthReportEntry> Entries { get; set; }
    }
}
