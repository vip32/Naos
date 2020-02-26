namespace Microsoft.Extensions.Logging
{
    public static class LogKeys
    {
        public static readonly string Startup = "STRTUP";
        public static readonly string Shutdown = "SHTDWN";
        public static readonly string StartupTask = "STUTSK";
        public static readonly string InboundRequest = "INBREQ";
        public static readonly string InboundResponse = "INBRES";
        public static readonly string OutboundRequest = "OUTREQ";
        public static readonly string OutboundResponse = "OUTRES";
        public static readonly string Application = "APPLTN";
        public static readonly string Domain = "DOMAIN";
        public static readonly string Infrastructure = "INFRST";
        public static readonly string AppCommand = "APPCMD";
        public static readonly string DomainRepository = "DOMREP";
        public static readonly string DomainEvent = "DOMEVT";
        public static readonly string DomainSpecification = "DOMSPEC";
        public static readonly string AppMessaging = "APPMSG";
        public static readonly string JobScheduling = "JOBSCH";
        public static readonly string ServiceDiscovery = "SVCDSC";
        public static readonly string Operations = "OPRTNS";
        public static readonly string Authentication = "AUTHNT";
        public static readonly string FileStorage = "FILSTR";
        public static readonly string Queueing = "QUEUNG";
        //public static readonly string QueueingEventHandler = "QUEEVT";
        public static readonly string KeyValueStorage = "KVLSTR";
        public static readonly string Tracing = "TRACNG";
    }
}
