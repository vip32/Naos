namespace Naos.Sample.Customers.Infrastructure
{
    using AutoMapper;
    using Naos.Foundation;
    using Naos.Sample.Customers.Domain;

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
                c.CreateMap<Order, DtoOrder>()
                    .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
                    .ForMember(d => d.Customer, o => o.MapFrom(s => s.CustomerNumber))
                    .ForMember(d => d.Order, o => o.MapFrom(s => s.OrderNumber))
                    .ForMember(d => d.FirstName, o => o.MapFrom(s => s.FirstName))
                    .ForMember(d => d.LastName, o => o.MapFrom(s => s.LastName))
                    .ForMember(d => d.Location, o => o.MapFrom(s => s.Region))
                    .ForMember(d => d.Total, o => o.MapFrom(s => s.Total))
                    .ForMember(d => d.DeliveryStartDate, o => o.MapFrom(s => s.DeliveryPeriod != null ? s.DeliveryPeriod.StartDate : null))
                    .ForMember(d => d.DeliveryEndDate, o => o.MapFrom(s => s.DeliveryPeriod != null ? s.DeliveryPeriod.EndDate : null))
                    .ForMember(d => d.ReturnStartDate, o => o.MapFrom(s => s.ReturnPeriod != null ? s.ReturnPeriod.StartDate : null))
                    .ForMember(d => d.ReturnEndDate, o => o.MapFrom(s => s.ReturnPeriod != null ? s.ReturnPeriod.EndDate : null))
                    .ForMember(d => d.TenantId, o => o.MapFrom(s => s.TenantId))
                    .ForMember(d => d.State, o => o.Ignore());

                c.CreateMap<DtoOrder, Order>()
                    .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
                    .ForMember(d => d.CustomerNumber, o => o.MapFrom(s => s.Customer)) // can map to private set
                    .ForMember(d => d.OrderNumber, o => o.MapFrom(s => s.Order))
                    .ForMember(d => d.FirstName, o => o.MapFrom(s => s.FirstName))
                    .ForMember(d => d.LastName, o => o.MapFrom(s => s.LastName))
                    .ForMember(d => d.Region, o => o.MapFrom(s => s.Location))
                    .ForMember(d => d.DeliveryPeriod, o => o.MapFrom(s => Period.Create(s.DeliveryStartDate, s.DeliveryEndDate))) // can map to private set
                    .ForMember(d => d.ReturnPeriod, o => o.MapFrom(s => Period.Create(s.ReturnStartDate, s.ReturnEndDate))) // can map to private set
                    .ForMember(d => d.TenantId, o => o.MapFrom(s => s.TenantId))
                    .ForMember(d => d.Total, o => o.MapFrom(s => s.Total));
            });

            configuration.AssertConfigurationIsValid();
            return configuration.CreateMapper();
        }
    }
}
