namespace Naos.Core.Common
{
    public struct LogEventIdentifiers
    {
        public const string InboundRequest = "INBREQ";
        public const string InboundResponse = "INBRES";
        public const string OutboundRequest = "OUTREQ";
        public const string OutboundResponse = "OUTRES";

        public const string AppCommand = "APCMND";
        public const string DomainRepository = "DOREPO";
        public const string DomainEvent = "DOEVNT";
        public const string Messaging = "MESSAG";
        public const string JobScheduling = "JOBSCH";
        public const string ServiceDiscovery = "SVCDSC";
        public const string Operations = "OPRTNS";
        public const string Authentication = "AUTHENT";
    }
}
