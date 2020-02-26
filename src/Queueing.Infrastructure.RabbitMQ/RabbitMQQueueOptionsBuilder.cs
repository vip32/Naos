namespace Naos.Queueing.Infrastructure
{
    using System;
    using MediatR;
    using Naos.Foundation;
    using Naos.Tracing.Domain;

    public class RabbitMQQueueOptionsBuilder :
       BaseOptionsBuilder<RabbitMQQueueOptions, RabbitMQQueueOptionsBuilder>
    {
        public RabbitMQQueueOptionsBuilder Tracer(ITracer tracer)
        {
            this.Target.Tracer = tracer;
            return this;
        }

        public RabbitMQQueueOptionsBuilder Mediator(IMediator mediator)
        {
            this.Target.Mediator = mediator;
            return this;
        }

        public RabbitMQQueueOptionsBuilder Serializer(ISerializer serializer)
        {
            this.Target.Serializer = serializer;

            return this;
        }

        public RabbitMQQueueOptionsBuilder QueueName(string name)
        {
            if (!name.IsNullOrEmpty())
            {
                this.Target.QueueName = name;
                // if (this.Target.MessageScope.IsNullOrEmpty())
                // {
                //     this.Target.MessageScope = name;
                // }
            }

            return this;
        }

        public RabbitMQQueueOptionsBuilder Provider(IRabbitMQProvider provider)
        {
            this.Target.Provider = provider;
            return this;
        }

        public RabbitMQQueueOptionsBuilder ExchangeName(string value)
        {
            if (!value.IsNullOrEmpty())
            {
                this.Target.ExchangeName = value;
            }

            return this;
        }

        public RabbitMQQueueOptionsBuilder RetryCount(int value)
        {
            this.Target.RetryCount = value;
            return this;
        }
    }
}
