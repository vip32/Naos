namespace Naos.Tracing.Domain
{
    /// <summary>
    /// Standard tag keys https://github.com/opentracing/specification/blob/master/semantic_conventions.md#span-tags-table
    /// </summary>
    public struct SpanTagKey
    {
        /// <summary>
        /// Span tag key to describe the type of sampler used on the root span.
        /// </summary>
        public const string SamplerType = "sampler.type";

        /// <summary>
        /// Span tag key to describe the parameter of the sampler used on the root span.
        /// </summary>
        public const string SamplerParam = "sampler.param";

        /// <summary>
        /// The software package, framework, library, or module that generated the associated Span. E.g., "grpc", "django", "JDBI".
        /// </summary>
        public const string Component = "component";

        /// <summary>
        /// HTTP method of the request for the associated Span. E.g., "GET", "POST"
        /// </summary>
        public const string HttpMethod = "http.method";

        /// <summary>
        /// HTTP url for the associated Span.
        /// </summary>
        public const string HttpUrl = "http.url";

        /// <summary>
        /// HTTP url host for the associated Span.
        /// </summary>
        public const string HttpHost = "http.host";

        /// <summary>
        /// HTTP url path for the associated Span.
        /// </summary>
        public const string HttpPath = "http.path";

        /// <summary>
        /// HTTP url route for the associated Span.
        /// </summary>
        public const string HttpRoute = "http.route";

        /// <summary>
        /// HTTP request size for the associated Span.
        /// </summary>
        public const string HttpRequestSize = "http.request.size";

        /// <summary>
        /// HTTP response size for the associated Span.
        /// </summary>
        public const string HttpResponseSize = "http.response.size";

        /// <summary>
        /// HTTP response status code for the associated Span. E.g., 200, 503, 404
        /// </summary>
        public const string HttpStatusCode = "http.status_code";

        /// <summary>
        /// HTTP request id for the associated Span. client or server
        /// </summary>
        public const string HttpRequestId = "http.requestid";

        /// <summary>
        /// Either "client" or "server" for the appropriate roles in an RPC, and "producer" or "consumer" for the appropriate roles in a messaging scenario.
        /// </summary>
        public const string SpanKind = "span.kind";

        /// <summary>
        /// An address at which messages can be exchanged. E.g. A Kafka record has an associated "topic name" that can be extracted by the instrumented producer or consumer and stored using this tag.
        /// </summary>
        public const string MessageBusDesitnation = "message_bus.destination";

        /// <summary>
        /// Database instance name.
        /// </summary>
        public const string DbInstance = "db.instance";

        /// <summary>
        /// A database statement for the given database type.
        /// </summary>
        public const string DbStatement = "db.statement";

        /// <summary>
        /// Database type. For any SQL database, "sql". For others, the lower-case database category, e.g. "cassandra", "hbase", or "redis".
        /// </summary>
        public const string DbType = "db.type";

        /// <summary>
        /// Username for accessing database.
        /// </summary>
        public const string DbUser = "db.user";

        /// <summary>
        /// true if and only if the application considers the operation represented by the Span to have failed
        /// </summary>
        public const string Error = "error";

        /// <summary>
        /// Remote "address", suitable for use in a networking client library. This may be a "ip:port", a bare "hostname", a FQDN, or even a JDBC substring like "mysql://prod-db:3306"
        /// </summary>
        public const string PeerAddress = "peer.address";

        /// <summary>
        /// Remote hostname. E.g., "opentracing.io", "internal.dns.name"
        /// </summary>
        public const string PeerHostname = "peer.hostname";

        /// <summary>
        /// Remote IPv4 address as a .-separated tuple. E.g., "127.0.0.1"
        /// </summary>
        public const string PeerIpv4 = "peer.ipv4";

        /// <summary>
        /// Remote IPv6 address as a string of colon-separated 4-char hex tuples. E.g., "2001:0db8:85a3:0000:0000:8a2e:0370:7334"
        /// </summary>
        public const string PeerIpv6 = "peer.ipv6";

        /// <summary>
        /// Remote port. E.g., 80
        /// </summary>
        public const string PeerPort = "peer.port";

        /// <summary>
        /// Remote service name (for some unspecified definition of "service"). E.g., "elasticsearch", "a_custom_microservice", "memcache"
        /// </summary>
        public const string PeerService = "peer.service";
    }
}
