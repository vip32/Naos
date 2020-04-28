namespace Naos.Foundation.Domain
{
    using System;

    [Serializable]
    public class EventSourcingRepositoryException : Exception
    {
        public EventSourcingRepositoryException()
        {
        }

        public EventSourcingRepositoryException(string message)
            : base(message)
        {
        }

        public EventSourcingRepositoryException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected EventSourcingRepositoryException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
