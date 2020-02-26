namespace Naos.Tracing.Infrastructure
{
    using AutoMapper;
    using Naos.Tracing.Domain;

    public static class MongoMapperFactory
    {
        public static IMapper Create() // automapper based mapper because of the needed expression mapping
        {
            var configuration = new MapperConfiguration(c =>
            {
                //c.IgnoreUnmapped();
                c.CreateMap<LogTrace, MongoLogTrace>()
                    //.IgnoreAllUnmapped()
                    .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
                    .ForMember(d => d.Level, o => o.MapFrom(s => s.Level))
                    .ForMember(d => d.Timestamp, o => o.MapFrom(s => s.Timestamp))
                    .ForMember(d => d.RenderedMessage, o => o.MapFrom(s => s.Message))
                    .ForPath(d => d.Properties.logKey, o => o.MapFrom(s => s.Key))
                    .ForPath(d => d.Properties.sourceContext, o => o.MapFrom(s => s.SourceContext))
                    .ForPath(d => d.Properties.ns_env, o => o.MapFrom(s => s.Environment))
                    .ForPath(d => d.Properties.ns_corid, o => o.MapFrom(s => s.Environment))
                    .ForPath(d => d.Properties.ns_ticks, o => o.MapFrom(s => s.Ticks))
                    .ForPath(d => d.Properties.ns_svcname, o => o.MapFrom(s => s.ServiceName))
                    .ForPath(d => d.Properties.ns_svcprod, o => o.MapFrom(s => s.ServiceProduct))
                    .ForPath(d => d.Properties.ns_svccapa, o => o.MapFrom(s => s.ServiceCapability))
                    .ForPath(d => d.Properties.ns_trktyp, o => o.MapFrom(s => s.TrackType))
                    .ForPath(d => d.Properties.ns_trkid, o => o.MapFrom(s => s.TrackId))
                    .ForPath(d => d.Properties.ns_trkid, o => o.MapFrom(s => s.SpanId))
                    .ForPath(d => d.Properties.ns_trkpid, o => o.MapFrom(s => s.ParentSpanId))
                    .ForPath(d => d.Properties.ns_trktid, o => o.MapFrom(s => s.TraceId))
                    .ForPath(d => d.Properties.ns_trknm, o => o.MapFrom(s => s.OperationName))
                    .ForPath(d => d.Properties.ns_trkknd, o => o.MapFrom(s => s.Kind))
                    .ForPath(d => d.Properties.ns_trksts, o => o.MapFrom(s => s.Status))
                    .ForPath(d => d.Properties.ns_trkstd, o => o.MapFrom(s => s.StatusDescription))
                    .ForPath(d => d.Properties.ns_trkstt, o => o.MapFrom(s => s.StartTime))
                    .ForPath(d => d.Properties.ns_trkent, o => o.MapFrom(s => s.EndTime));

                c.CreateMap<MongoLogTrace, LogTrace>()
                    //.IgnoreAllUnmapped()
                    .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
                    .ForMember(d => d.Level, o => o.MapFrom(s => s.Level))
                    .ForMember(d => d.Timestamp, o => o.MapFrom(s => s.Timestamp))
                    .ForMember(d => d.Message, o => o.MapFrom(s => s.RenderedMessage))
                    .ForMember(d => d.Key, o => o.MapFrom(s => s.Properties.logKey))
                    .ForMember(d => d.SourceContext, o => o.MapFrom(s => s.Properties.sourceContext))
                    .ForMember(d => d.Environment, o => o.MapFrom(s => s.Properties.ns_env))
                    .ForMember(d => d.CorrelationId, o => o.MapFrom(s => s.Properties.ns_corid))
                    .ForMember(d => d.Ticks, o => o.MapFrom(s => s.Properties.ns_ticks))
                    .ForMember(d => d.ServiceName, o => o.MapFrom(s => s.Properties.ns_svcname))
                    .ForMember(d => d.ServiceProduct, o => o.MapFrom(s => s.Properties.ns_svcprod))
                    .ForMember(d => d.ServiceCapability, o => o.MapFrom(s => s.Properties.ns_svccapa))
                    .ForMember(d => d.TrackType, o => o.MapFrom(s => s.Properties.ns_trktyp))
                    .ForMember(d => d.TrackId, o => o.MapFrom(s => s.Properties.ns_trkid))
                    .ForMember(d => d.SpanId, o => o.MapFrom(s => s.Properties.ns_trkid))
                    .ForMember(d => d.ParentSpanId, o => o.MapFrom(s => s.Properties.ns_trkpid))
                    .ForMember(d => d.TraceId, o => o.MapFrom(s => s.Properties.ns_trktid))
                    .ForMember(d => d.OperationName, o => o.MapFrom(s => s.Properties.ns_trknm))
                    .ForMember(d => d.Kind, o => o.MapFrom(s => s.Properties.ns_trkknd))
                    .ForMember(d => d.Status, o => o.MapFrom(s => s.Properties.ns_trksts))
                    .ForMember(d => d.StatusDescription, o => o.MapFrom(s => s.Properties.ns_trkstd))
                    .ForMember(d => d.StartTime, o => o.MapFrom(s => s.Properties.ns_trkstt))
                    .ForMember(d => d.EndTime, o => o.MapFrom(s => s.Properties.ns_trkent))
                    .ForMember(d => d.Tags, o => o.Ignore())
                    .ForMember(d => d.Logs, o => o.Ignore())
                    .ForMember(d => d.DomainEvents, o => o.Ignore())
                    .ForMember(d => d.SourceContext, o => o.Ignore());
            });

            configuration.AssertConfigurationIsValid();
            return configuration.CreateMapper();
        }
    }
}