namespace Naos.Queueing.Domain
{
    using System;
    using Naos.Foundation;
    using Naos.Tracing.Domain;

    public class QueueOptionsBase : OptionsBase
    {
        public ITracer Tracer { get; set; }

        public string QueueName { get; set; }

        public int Retries { get; set; } = 3;

        public TimeSpan ProcessInterval { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// The default message time to live.
        /// </summary>
        public TimeSpan? Expiration { get; set; }

        public ISerializer Serializer { get; set; }
    }
}
