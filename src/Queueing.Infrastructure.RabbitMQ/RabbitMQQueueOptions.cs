namespace Naos.Queueing.Infrastructure
{
    using System;
    using MediatR;
    using Naos.Queueing.Domain;

    public class RabbitMQQueueOptions : QueueOptionsBase
    {
        public IMediator Mediator { get; set; }

        public IRabbitMQProvider Provider { get; set; }

        public string ExchangeName { get; set; } = "naos_queueing";

        public int RetryCount { get; set; } = 3;

        public TimeSpan DequeueInterval { get; set; } = TimeSpan.FromSeconds(2);
    }
}
