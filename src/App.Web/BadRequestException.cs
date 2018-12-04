namespace Naos.Core.App.Web
{
    using System;
    using System.Runtime.Serialization;
    using Microsoft.AspNetCore.Mvc.ModelBinding;

    /// <summary>
    /// Bad request exception type for exceptions thrown by Naos api controllers
    /// </summary>
    [Serializable]
    public class BadRequestException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BadRequestException"/> class.
        /// </summary>
        public BadRequestException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BadRequestException"/> class.
        /// </summary>
        /// <param name="serializationInfo">The serialization information.</param>
        /// <param name="context">The context.</param>
        public BadRequestException(SerializationInfo serializationInfo, StreamingContext context)
            : base(serializationInfo, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BadRequestException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public BadRequestException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BadRequestException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public BadRequestException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public BadRequestException(ModelStateDictionary modelState)
        {
            this.ModelState = modelState;
        }

        public ModelStateDictionary ModelState { get; }
    }
}
