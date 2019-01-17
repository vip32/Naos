namespace Naos.Core.Common
{
    public struct LogEventPropertyKeys
    {
        public const string CorrelationId = "naos_corid";
        public const string RequestId = "naos_reqid";
        public const string TenantId = "naos_tntid";
        public const string Environment = "naos_env";

        public const string ServiceProduct = "naos_svcprod";
        public const string ServiceCapability = "naos_svccapa";
        public const string ServiceName = "naos_svcname";

        public const string TrackType = "naos_trcktyp";
        public const string TrackDomainEvent = "naos_trckdom";
        public const string TrackCommand = "naos_trckcom";
        public const string TrackMessage = "naos_trckmsg";
        public const string TrackInboundRequest = "naos_trckibr";
        public const string TrackOutboundRequest = "naos_trckobr";
    }
}
