namespace Naos.Foundation.Domain.EventSourcing
{
    using System;

#pragma warning disable SA1402 // File may only contain a single type
    [Serializable]
    public class EventStoreException : Exception
    {
        public EventStoreException()
        {
        }

        public EventStoreException(string message)
            : base(message)
        {
        }

        public EventStoreException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected EventStoreException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }

    [Serializable]
    public class EventStoreStreamNotFoundException : EventStoreException
    {
        public EventStoreStreamNotFoundException()
        {
        }

        public EventStoreStreamNotFoundException(string message)
            : base(message)
        {
        }

        public EventStoreStreamNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected EventStoreStreamNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }

    [Serializable]
    public class EventStoreCommunicationException : EventStoreException
    {
        public EventStoreCommunicationException()
        {
        }

        public EventStoreCommunicationException(string message)
            : base(message)
        {
        }

        public EventStoreCommunicationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected EventStoreCommunicationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
