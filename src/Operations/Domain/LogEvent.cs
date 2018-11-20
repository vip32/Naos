namespace Naos.Core.Operations.Domain
{
    using System;
    using System.Collections.Generic;
    using Naos.Core.Domain;

    public class LogEvent : Entity<string>
    {
        public string Level { get; set; }

        public string Message { get; set; }

        public DateTime Timestamp { get; set; }

        public Dictionary<string, object> Properties { get; set; }
    }
}
