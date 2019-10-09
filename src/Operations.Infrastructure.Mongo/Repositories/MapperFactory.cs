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
                // TODO: try reversemap https://stackoverflow.com/questions/13490456/automapper-bidirectional-mapping-with-reversemap-and-formember
                //c.IgnoreUnmapped();
                c.CreateMap<LogEvent, MongoLogEvent>()
                    .ForMember(d => d.Properties.ns_ticks, o => o.MapFrom(s => s.Ticks));
                // TODO: more mappings

                c.CreateMap<MongoLogEvent, LogEvent>()
                    .ForMember(d => d.Ticks, o => o.MapFrom(s => s.Properties.ns_ticks));
                // TODO: more mappings
            });

            configuration.AssertConfigurationIsValid();
            return configuration.CreateMapper();
        }
    }
}