namespace Naos.Queueing
{
    using System;
    using MediatR;
    using Naos.Foundation;

    public class InMemoryQueueOptionsBuilder :
        BaseOptionsBuilder<InMemoryQueueOptions, InMemoryQueueOptionsBuilder>
    {
        public InMemoryQueueOptionsBuilder Mediator(IMediator mediator)
        {
            this.Target.Mediator = mediator;
            return this;
        }

        public InMemoryQueueOptionsBuilder Name(string name)
        {
            this.Target.QueueName = name;
            return this;
        }

        public InMemoryQueueOptionsBuilder Retries(int retries)
        {
            this.Target.Retries = retries;
            return this;
        }

        public InMemoryQueueOptionsBuilder ProcessInterval(TimeSpan timeout)
        {
            this.Target.ProcessInterval = timeout;
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

        public InMemoryQueueOptionsBuilder DequeueInterval(TimeSpan interval)
        {
            this.Target.DequeueInterval = interval;
            return this;
        }
    }
}
