namespace Naos.Core.Common
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Base exception type for exceptions thrown by Naos
    /// </summary>
    [Serializable]
    public class NaosException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NaosException"/> class.
        /// </summary>
        public NaosException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NaosException"/> class.
        /// </summary>
        /// <param name="serializationInfo">The serialization information.</param>
        /// <param name="context">The context.</param>
        public NaosException(SerializationInfo serializationInfo, StreamingContext context)
            : base(serializationInfo, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NaosException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public NaosException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NaosException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public NaosException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
