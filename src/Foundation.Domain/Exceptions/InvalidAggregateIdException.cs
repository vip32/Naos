namespace Naos.Foundation.Domain
{
    using System;

    public class InvalidAggregateIdException : Exception
    {
        public InvalidAggregateIdException()
        {
        }

        public InvalidAggregateIdException(string message)
            : base(message)
        {
        }

        public InvalidAggregateIdException(string message, Exception innerException)
        : base(message, innerException)
        {
        }
    }
}
