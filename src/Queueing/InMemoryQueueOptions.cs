namespace Naos.Core.Queueing
{
    using System;
    using Naos.Core.Queueing.Domain;

    public class InMemoryQueueOptions : QueueBaseOptions
    {
        public TimeSpan RetryDelay { get; set; } = TimeSpan.FromMinutes(1);

        public int[] RetryMultipliers { get; set; } = { 1, 3, 5, 10 };

        public TimeSpan DequeueInterval { get; set; } = TimeSpan.FromSeconds(2);
    }
}
