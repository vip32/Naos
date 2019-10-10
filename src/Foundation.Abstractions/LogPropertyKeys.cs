namespace Microsoft.Extensions.Logging
{
    public struct LogPropertyKeys
    {
        public const string LogKey = "LogKey";
        public const string EventType = "ns_evntp";
        public const string Ticks = "ns_ticks";
        public const string CorrelationId = "ns_corid";
        public const string Id = "ns_id";
        public const string RequestId = "ns_reqid";
        public const string TenantId = "ns_tntid";
        public const string Environment = "ns_env";

        public const string ServiceProduct = "ns_svcprod";
        public const string ServiceCapability = "ns_svccapa";
        public const string ServiceName = "ns_svcname";

        public const string TrackType = "ns_trktyp";
        public const string TrackName = "ns_trknm";
        public const string TrackTraceId = "ns_trktid";
        public const string TrackId = "ns_trkid";
        public const string TrackParentId = "ns_trkpid";
        public const string TrackKind = "ns_trkknd";
        public const string TrackStatus = "ns_trksts";
        public const string TrackStatusDescription = "ns_trkstd";
        public const string TrackStartTime = "ns_trkstt";
        public const string TrackEndTime = "ns_trkent";
        public const string TrackDuration = "ns_trkdur";
        public const string TrackTimestamp = "ns_trktsp";
        public const string TrackMisc = "ns_trkmsc";
        public const string TrackSendDomainEvent = "ns_trksdm";
        public const string TrackHandleDomainEvent = "ns_trkhdm";
        public const string TrackSendCommand = "ns_trkscm";
        public const string TrackHandleCommand = "ns_trkhcm";
        public const string TrackSubscribeMessage = "ns_trksmg";
        public const string TrackPublishMessage = "ns_trkpmg";
        public const string TrackReceiveMessage = "ns_trkrmg";
        public const string TrackStartJob = "ns_trksjb";
        public const string TrackFinishJob = "ns_trkfjb";
        public const string TrackOutboundRequest = "ns_trkorq"; // client
        public const string TrackOutboundResponse = "ns_trkors";
        public const string TrackInboundRequest = "ns_trkirq"; // server
        public const string TrackInboundResponse = "ns_trkirs";
        public const string TrackEnqueue = "ns_trkenq";
        public const string TrackDequeue = "ns_trkdeq";
    }
}
