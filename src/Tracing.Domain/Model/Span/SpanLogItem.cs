namespace Naos.Tracing.Domain
{
    using System;

    public struct SpanLogItem
    {
        public DateTimeOffset Timestamp { get; set; }

        public string Key { get; set; }

        public string Message { get; set; }

        public override string ToString()
        {
            return $"{this.Timestamp:o} [{this.Key}] {this.Message}";
        }
    }
}