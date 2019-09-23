namespace Naos.Sample.Catalogs.Domain
{
    using System;
    using Naos.Foundation.Domain;

    public class Product : AggregateRoot<string>, ITenantEntity
    {
        public string Number { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Type { get; set; }

        public decimal Price { get; set; }

        public string Region { get; set; }

        public string TenantId { get; set; }

        public Guid CategoryId { get; set; }

        public string CatalogName { get; set; }

        public bool HasStock { get; set; }
    }
}
