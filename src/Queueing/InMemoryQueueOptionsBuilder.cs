namespace Naos.Core.Queueing
{
    using System;
    using Naos.Core.Common;
    using Naos.Core.Common.Serialization;

    public class InMemoryQueueOptionsBuilder :
        BaseOptionsBuilder<InMemoryQueueOptions, InMemoryQueueOptionsBuilder>
    {
        public InMemoryQueueOptionsBuilder Name(string name)
        {
            this.Target.Name = name;
            return this;
        }

        public InMemoryQueueOptionsBuilder Retries(int retries)
        {
            this.Target.Retries = retries;
            return this;
        }

        public InMemoryQueueOptionsBuilder ProcessTimeout(TimeSpan timeout)
        {
            this.Target.ProcessTimeout = timeout;
            return this;
        }

        public InMemoryQueueOptionsBuilder Serializer(ISerializer serializer)
        {
            this.Target.Serializer = serializer;
            return this;
        }

        public InMemoryQueueOptionsBuilder RetryDelay(TimeSpan retryDelay)
        {
            this.Target.RetryDelay = retryDelay;
            return this;
        }

        public InMemoryQueueOptionsBuilder RetryMultipliers(int[] multipliers = null)
        {
            this.Target.RetryMultipliers = multipliers ?? throw new ArgumentNullException(nameof(multipliers));
            return this;
        }

        public InMemoryQueueOptionsBuilder DequeueInterval(TimeSpan dequeueInterval)
        {
            this.Target.DequeueInterval = dequeueInterval;
            return this;
        }
    }
}
