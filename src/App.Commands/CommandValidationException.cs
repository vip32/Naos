namespace Naos.Core.App.Commands
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using FluentValidation;
    using FluentValidation.Results;

    /// <summary>
    /// Standard service exception
    /// </summary>
    /// <seealso cref="System.ApplicationException" />
    public class CommandValidationException : ValidationException
    {
        public CommandValidationException(string message)
            : base(message)
        {
        }

        public CommandValidationException(string message, IEnumerable<ValidationFailure> errors)
            : base(message, errors)
        {
        }

        public CommandValidationException(IEnumerable<ValidationFailure> errors)
            : base(errors)
        {
        }

        public CommandValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
