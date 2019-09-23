namespace Naos.Queueing
{
    using System;
    using MediatR;
    using Naos.Queueing.Domain;

    public class InMemoryQueueOptions : BaseQueueOptions
    {
        public IMediator Mediator { get; set; }

        public TimeSpan RetryDelay { get; set; } = TimeSpan.FromMinutes(1);

        public int[] RetryMultipliers { get; set; } = { 1, 3, 5, 10 };

        public TimeSpan DequeueInterval { get; set; } = TimeSpan.FromSeconds(2);
    }
}
