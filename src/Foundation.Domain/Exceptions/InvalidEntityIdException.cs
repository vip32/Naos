namespace Naos.Foundation.Domain
{
    using System;

    public class InvalidEntityIdException : Exception
    {
        public InvalidEntityIdException()
        {
        }

        public InvalidEntityIdException(string message)
            : base(message)
        {
        }

        public InvalidEntityIdException(string message, Exception innerException)
        : base(message, innerException)
        {
        }
    }
}
