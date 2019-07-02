namespace Naos.Core.Operations.Domain
{
    public enum SpanKind
    {
        /// <summary>
        /// Span kind was not specified.
        /// </summary>
        Internal = 1,

        /// <summary>
        /// Server span represents request incoming from external component.
        /// </summary>
        Server = 2,

        /// <summary>
        /// Client span represents outgoing request to the external component.
        /// </summary>
        Client = 3,

        /// <summary>
        /// Producer span represents output provided to external components. Unlike client and
        /// server, there is no direct critical path latency relationship between producer and consumer
        /// spans.
        /// </summary>
        Producer = 4,

        /// <summary>
        /// Consumer span represents output received from an external component. Unlike client and
        /// server, there is no direct critical path latency relationship between producer and consumer
        /// spans.
        /// </summary>
        Consumer = 5,
    }
}

//       1---2---4 (producer/consumer)
//     / | \
//    1a |  1c     (internal/client/server) *
//       1b