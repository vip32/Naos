namespace Naos.Core.Common
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Exception type for client formating exceptions thrown by Naos. Should be used when user/client input was not in the expected format.
    /// </summary>
    [Serializable]
    public class NaosClientFormatException : NaosException
    {
        public NaosClientFormatException()
        {
        }

        public NaosClientFormatException(string message)
            : base(message)
        {
        }

        public NaosClientFormatException(SerializationInfo serializationInfo, StreamingContext context)
            : base(serializationInfo, context)
        {
        }

        public NaosClientFormatException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
