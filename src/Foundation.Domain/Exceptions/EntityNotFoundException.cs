namespace Naos.Foundation.Domain
{
    using System;

    [Serializable]
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException()
            : base()
        {
        }

        public EntityNotFoundException(string message)
            : base(message)
        {
        }

        public EntityNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected EntityNotFoundException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext context)
            : base(serializationInfo, context)
        {
        }
    }
}
