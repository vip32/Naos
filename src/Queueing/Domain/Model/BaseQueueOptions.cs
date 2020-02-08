namespace Naos.Queueing.Domain
{
    using System;
    using Naos.Foundation;
    using Naos.Tracing.Domain;

    public class BaseQueueOptions : BaseOptions
    {
        public ITracer Tracer { get; set; }

        public string QueueName { get; set; }

        public int Retries { get; set; } = 3;

        public TimeSpan ProcessInterval { get; set; } = TimeSpan.FromSeconds(10);

        public ISerializer Serializer { get; set; }
    }
}
