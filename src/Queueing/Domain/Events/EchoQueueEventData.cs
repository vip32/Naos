﻿namespace Naos.Core.Queueing.Domain
{
    using Naos.Foundation.Domain;

    public class EchoQueueEventData : IHaveCorrelationId
    {
        public string Text { get; set; }

        public string CorrelationId { get; set; }
    }
}
