namespace Naos.Core.Common
{
    public struct LogEventPropertyKeys
    {
        public const string Ticks = "ns_ticks";
        public const string CorrelationId = "ns_corid";
        public const string RequestId = "ns_reqid";
        public const string TenantId = "ns_tntid";
        public const string Environment = "ns_env";

        public const string ServiceProduct = "ns_svcprod";
        public const string ServiceCapability = "ns_svccapa";
        public const string ServiceName = "ns_svcname";

        public const string TrackType = "ns_trktyp";
        public const string TrackMisc = "ns_trkmsc";
        public const string TrackSendDomainEvent = "ns_trksdm";
        public const string TrackHandleDomainEvent = "ns_trkhdm";
        public const string TrackSendCommand = "ns_trkscm";
        public const string TrackHandleCommand = "ns_trkhcm";
        public const string TrackPublishMessage = "ns_trkpmg";
        public const string TrackHandleMessage = "ns_trkhmg";
        public const string TrackStartJob = "ns_trksjb";
        public const string TrackFinishJob = "ns_trkfjb";
        public const string TrackInboundRequest = "ns_trkirq";
        public const string TrackInboundResponse = "ns_trkirs";
        public const string TrackOutboundRequest = "ns_trkorq";
        public const string TrackOutboundResponse = "ns_trkors";
    }
}
