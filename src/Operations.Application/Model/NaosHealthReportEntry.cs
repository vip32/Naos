namespace Naos.Operations.Application
{
    using System;
    using System.Collections.Generic;

    public class NaosHealthReportEntry
    {
        public string Status { get; set; }

        public string Description { get; set; }

        public TimeSpan Duration { get; set; }

        public string Took { get; set; }

        public string Error { get; set; }

        public IReadOnlyDictionary<string, object> Data { get; set; }

        public IEnumerable<string> Tags { get; set; }
    }
}
