namespace Naos.Core.Operations.Domain
{
    public class TraceEvent : LogEvent
    {
        public string Key { get; set; } // shared id (for example request id)

        public string Name { get; set; } // api/customer or http/message/command

        public long Duration { get; set; } // 234.3242

        public long DurationText { get;  } // 234 milliseconds

        public int StatusCode { get; set; } // 200
    }
}
