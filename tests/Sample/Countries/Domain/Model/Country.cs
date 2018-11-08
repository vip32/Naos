namespace Naos.Sample.Countries.Domain
{
    using System.Collections.Generic;
    using Naos.Core.Domain;

    public class Country : Entity<string>, ITenantEntity
    {
        public string Name { get; set; }

        public string Code { get; set; } // ISO 3166 country code

        public IEnumerable<string> LanguageCodes { get; set; } // ISO 639-1 language codes

        public string TenantId { get; set; }
    }
}
