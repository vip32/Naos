namespace Naos.Sample.UserAccounts.Infrastructure
{
    using AutoMapper;
    using Naos.Foundation.Domain;
    using Naos.Sample.UserAccounts.Domain;

    public static class MapperFactory
    {
        public static IMapper Create()
        {
            var configuration = new MapperConfiguration(c =>
            {
                // TODO: try reversemap https://stackoverflow.com/questions/13490456/automapper-bidirectional-mapping-with-reversemap-and-formember
                //c.AddExpressionMapping();
                c.IgnoreUnmapped();
                //c.AllowNullCollections = true;
                c.CreateMap<UserVisit, DtoUserVisit>()
                    .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
                    .ForMember(d => d.EmailAddress, o => o.MapFrom(s => s.Email))
                    .ForMember(d => d.Location, o => o.MapFrom(s => s.Region))
                    .ForMember(d => d.Timestamp, o => o.MapFrom(s => s.Timestamp))
                    .ForMember(d => d.OwnerId, o => o.MapFrom(s => s.TenantId));

                c.CreateMap<DtoUserVisit, UserVisit>()
                    .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
                    .ForMember(d => d.Email, o => o.MapFrom(s => s.EmailAddress))
                    .ForMember(d => d.Region, o => o.MapFrom(s => s.Location))
                    .ForMember(d => d.Timestamp, o => o.MapFrom(s => s.Timestamp))
                    .ForMember(d => d.TenantId, o => o.MapFrom(s => s.OwnerId));
            });

            configuration.AssertConfigurationIsValid();
            return configuration.CreateMapper();
        }
    }
}
