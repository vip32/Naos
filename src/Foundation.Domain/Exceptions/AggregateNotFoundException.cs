namespace Naos.Foundation.Domain
{
    using System;

    [Serializable]
    public class AggregateNotFoundException : Exception
    {
        public AggregateNotFoundException()
            : base()
        {
        }

        public AggregateNotFoundException(string message)
            : base(message)
        {
        }

        public AggregateNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected AggregateNotFoundException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext context)
            : base(serializationInfo, context)
        {
        }
    }
}
