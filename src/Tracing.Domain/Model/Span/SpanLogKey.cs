namespace Naos.Tracing.Domain
{
    /// <summary>
    /// Standard log keys https://github.com/opentracing/specification/blob/master/semantic_conventions.md#log-fields-table
    /// </summary>
    public struct SpanLogKey
    {
        /// <summary>
        /// The type or "kind" of an error (only for event="error" logs). E.g., "Exception", "OSError"
        /// </summary>
        public const string ErrorKind = "error.kind";

        /// <summary>
        /// The actual Throwable/Exception/Error object instance itself. E.g., A System.UnsupportedOperationException instance
        /// </summary>
        public const string ErrorObject = "error.object";

        /// <summary>
        /// A concise, human-readable, one-line message explaining the event. E.g., "Could not connect to backend", "Cache invalidation succeeded"
        /// </summary>
        public const string Message = "message";

        /// <summary>
        /// A stable identifier for some notable moment in the lifetime of a Span.
        /// </summary>
        public const string Event = "event";

        /// <summary>
        /// A stack trace in platform-conventional format; may or may not pertain to an error.
        /// </summary>
        public const string StackTrace = "stack";
    }
}