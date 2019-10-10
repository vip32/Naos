namespace Naos.Sample.Inventory.Infrastructure
{
    using AutoMapper;
    using Naos.Sample.Inventory.Domain;

    public static class MapperFactory
    {
        public static IMapper Create()
        {
            var configuration = new MapperConfiguration(c =>
            {
                // TODO: try reversemap https://stackoverflow.com/questions/13490456/automapper-bidirectional-mapping-with-reversemap-and-formember
                //c.AddExpressionMapping();
                //c.IgnoreUnmapped();
                //c.AllowNullCollections = true;
                c.CreateMap<ProductReplenishment, DtoProductReplenishment>()
                    .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
                    .ForMember(d => d.IdentifierHash, o => o.MapFrom(s => s.IdentifierHash))
                    .ForMember(d => d.ProductNumber, o => o.MapFrom(s => s.Number))
                    .ForMember(d => d.Amount, o => o.MapFrom(s => s.Quantity))
                    .ForMember(d => d.Region, o => o.MapFrom(s => s.Region))
                    .ForMember(d => d.OwnerId, o => o.MapFrom(s => s.TenantId))
                    .ForMember(d => d.ShipDate, o => o.MapFrom(s => s.ShippedDate))
                    .ForMember(d => d.FromLocation, o => o.MapFrom(s => s.ShippedFromLocation))
                    .ForMember(d => d.ArriveDate, o => o.MapFrom(s => s.ArrivedDate))
                    .ForMember(d => d.AtLocation, o => o.MapFrom(s => s.ArrivedAtLocation))
                    .ForMember(d => d.State, o => o.Ignore());

                c.CreateMap<DtoProductReplenishment, ProductReplenishment>()
                    .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
                    .ForMember(d => d.IdentifierHash, o => o.MapFrom(s => s.IdentifierHash))
                    .ForMember(d => d.Number, o => o.MapFrom(s => s.ProductNumber))
                    .ForMember(d => d.Quantity, o => o.MapFrom(s => s.Amount))
                    .ForMember(d => d.Region, o => o.MapFrom(s => s.Region))
                    .ForMember(d => d.TenantId, o => o.MapFrom(s => s.OwnerId))
                    .ForMember(d => d.ShippedDate, o => o.MapFrom(s => s.ShipDate))
                    .ForMember(d => d.ShippedFromLocation, o => o.MapFrom(s => s.FromLocation))
                    .ForMember(d => d.ArrivedDate, o => o.MapFrom(s => s.ArriveDate))
                    .ForMember(d => d.ArrivedAtLocation, o => o.MapFrom(s => s.AtLocation));
            });

            configuration.AssertConfigurationIsValid();
            return configuration.CreateMapper();
        }
    }
}
