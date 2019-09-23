namespace Naos.Queueing.Domain
{
    using System.Diagnostics;

    [DebuggerDisplay("Queued={Queued}, Enqueued={Enqueued}, Completed={Completed}")]
    public class QueueMetrics
    {
        public long Queued { get; set; }

        public long Working { get; set; }

        public long Deadlettered { get; set; }

        public long Enqueued { get; set; }

        public long Dequeued { get; set; }

        public long Completed { get; set; }

        public long Abandoned { get; set; }

        public long Errors { get; set; }

        public long Timeouts { get; set; }
    }
}
