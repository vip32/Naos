namespace Naos.Core.Operations.Domain
{
    public enum SpanStatus
    {
        /// <summary>
        /// The operation is running.
        /// </summary>
        Transient = 0,

        /// <summary>
        /// The operation completed successfully.
        /// </summary>
        Succeeded = 1,

        /// <summary>
        /// The operation was cancelled (typically by the client/consumer).
        /// </summary>
        Cancelled = 2,

        /// <summary>
        /// The operation has failed (error).
        /// </summary>
        Failed = 3,
    }
}