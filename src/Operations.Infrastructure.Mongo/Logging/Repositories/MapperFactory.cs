namespace Naos.Operations.Infrastructure.Mongo
{
    using AutoMapper;
    using Naos.Operations.Domain;

    public static class MapperFactory
    {
        public static IMapper Create() // automapper based mapper because of the needed expression mapping
        {
            var configuration = new MapperConfiguration(c =>
            {
                c.CreateMap<LogEvent, MongoLogEvent>()
                    //.IgnoreAllUnmapped()
                    .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
                    .ForMember(d => d.Level, o => o.MapFrom(s => s.Level))
                    .ForMember(d => d.Timestamp, o => o.MapFrom(s => s.Timestamp))
                    .ForMember(d => d.RenderedMessage, o => o.MapFrom(s => s.Message))
                    .ForPath(d => d.Properties.logKey, o => o.MapFrom(s => s.Key))
                    .ForPath(d => d.Properties.sourceContext, o => o.MapFrom(s => s.SourceContext))
                    .ForPath(d => d.Properties.ns_env, o => o.MapFrom(s => s.Environment))
                    .ForPath(d => d.Properties.ns_corid, o => o.MapFrom(s => s.Environment))
                    .ForPath(d => d.Properties.ns_trktyp, o => o.MapFrom(s => s.TrackType))
                    .ForPath(d => d.Properties.ns_ticks, o => o.MapFrom(s => s.Ticks))
                    .ForPath(d => d.Properties.ns_svcname, o => o.MapFrom(s => s.ServiceName))
                    .ForPath(d => d.Properties.ns_svcprod, o => o.MapFrom(s => s.ServiceProduct))
                    .ForPath(d => d.Properties.ns_svccapa, o => o.MapFrom(s => s.ServiceCapability));

                c.CreateMap<MongoLogEvent, LogEvent>()
                    //.IgnoreAllUnmapped()
                    .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
                    .ForMember(d => d.Level, o => o.MapFrom(s => s.Level))
                    .ForMember(d => d.Timestamp, o => o.MapFrom(s => s.Timestamp))
                    .ForMember(d => d.Message, o => o.MapFrom(s => s.RenderedMessage))
                    .ForMember(d => d.Key, o => o.MapFrom(s => s.Properties.logKey))
                    .ForMember(d => d.SourceContext, o => o.MapFrom(s => s.Properties.sourceContext))
                    .ForMember(d => d.Environment, o => o.MapFrom(s => s.Properties.ns_env))
                    .ForMember(d => d.CorrelationId, o => o.MapFrom(s => s.Properties.ns_corid))
                    .ForMember(d => d.TrackType, o => o.MapFrom(s => s.Properties.ns_trktyp))
                    .ForMember(d => d.Ticks, o => o.MapFrom(s => s.Properties.ns_ticks))
                    .ForMember(d => d.ServiceName, o => o.MapFrom(s => s.Properties.ns_svcname))
                    .ForMember(d => d.ServiceProduct, o => o.MapFrom(s => s.Properties.ns_svcprod))
                    .ForMember(d => d.ServiceCapability, o => o.MapFrom(s => s.Properties.ns_svccapa))
                    .ForMember(d => d.DomainEvents, o => o.Ignore())
                    .ForMember(d => d.SourceContext, o => o.Ignore())
                    .ForMember(d => d.TrackId, o => o.Ignore());
            });

            configuration.AssertConfigurationIsValid();
            return configuration.CreateMapper();
        }
    }
}