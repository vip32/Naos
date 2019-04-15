namespace Naos.Core.Operations.Domain
{
    public class LogTraceEvent : LogEvent
    {
        public string Span { get; set; } // shared span identifier (for example request id)

        public string Name { get; set; } // http/job/message/command   >        message=api/customer or http/message/command

        public long Duration { get; set; } // 234.3242

        public long DurationText { get;  } // 234 milliseconds

        public int StatusCode { get; set; } // 200
    }
}
