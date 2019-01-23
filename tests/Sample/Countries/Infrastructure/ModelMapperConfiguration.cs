namespace Naos.Sample.Countries.Infrastructure
{
    using System.Collections.Generic;
    using AutoMapper;
    using Naos.Core.Common;
    using Naos.Sample.Countries.Domain;

    public static class ModelMapperConfiguration
    {
        public static IMapper Create()
        {
            var mapper = new MapperConfiguration(c =>
            {
                // TODO: try reversemap https://stackoverflow.com/questions/13490456/automapper-bidirectional-mapping-with-reversemap-and-formember
                //c.AddExpressionMapping();
                //c.IgnoreUnmapped();
                //c.AllowNullCollections = true;
                c.CreateMap<Country, DbCountry>()
                    .ForMember(d => d.ETag, o => o.MapFrom(s => s.IdentifierHash))
                    .ForMember(d => d.OwnerTenant, o => o.MapFrom(s => s.TenantId))
                    .ForMember(d => d.Identifier, o => o.MapFrom(s => s.Id))
                    .ForMember(d => d.CountryCode, o => o.MapFrom(s => s.Code))
                    .ForMember(d => d.CountryName, o => o.MapFrom(s => s.Name))
                    .ForMember(d => d.LanguageCodes, o => o.MapFrom(new LanguageCodesResolver()));

                c.CreateMap<DbCountry, Country>()
                    .ForMember(d => d.IdentifierHash, o => o.MapFrom(s => s.ETag))
                    .ForMember(d => d.TenantId, o => o.MapFrom(s => s.OwnerTenant))
                    .ForMember(d => d.Id, o => o.MapFrom(s => s.Identifier))
                    .ForMember(d => d.Code, o => o.MapFrom(s => s.CountryCode))
                    .ForMember(d => d.Name, o => o.MapFrom(s => s.CountryName))
                    .ForMember(d => d.LanguageCodes, o => o.MapFrom(new LanguageCodesResolver()))
                    .ForMember(d => d.State, o => o.Ignore());
            });

            mapper.AssertConfigurationIsValid();
            return mapper.CreateMapper();
        }

        private class LanguageCodesResolver : IValueResolver<Country, DbCountry, string>, IValueResolver<DbCountry, Country, IEnumerable<string>>
        {
            public string Resolve(Country source, DbCountry destination, string destMember, ResolutionContext context)
            {
                return source.LanguageCodes.Safe().ToString(";");
            }

            public IEnumerable<string> Resolve(DbCountry source, Country destination, IEnumerable<string> destMember, ResolutionContext context)
            {
                return source.LanguageCodes.Split(';');
            }
        }
    }
}
