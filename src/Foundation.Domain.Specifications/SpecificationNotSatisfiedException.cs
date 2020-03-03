namespace Naos.Foundation.Domain
{
    using System;

    [Serializable]
    public class SpecificationNotSatisfiedException : NaosException
    {
        public SpecificationNotSatisfiedException()
            : base()
        {
        }

        public SpecificationNotSatisfiedException(string message)
            : base(message)
        {
        }

        public SpecificationNotSatisfiedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected SpecificationNotSatisfiedException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext context)
            : base(serializationInfo, context)
        {
        }
    }
}
