namespace Naos.Commands.Application
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    /// <summary>
    /// Standard service exception.
    /// </summary>
    /// <seealso cref="ApplicationException" />
    public class CommandException : Exception
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandException"/> class.
        /// </summary>
        public CommandException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandException"/> class.
        /// </summary>
        /// <param name="message">A message that describes the error.</param>
        public CommandException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception. If the innerException parameter is not a null reference, the current exception is raised in a catch block that handles the inner exception.</param>
        public CommandException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandException"/> class.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected CommandException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"></see> that contains contextual information about the source or destination.</param>
        //[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
