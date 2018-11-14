namespace Naos.Sample.Countries.Infrastructure
{
    public class CountryDto
    {
        public string Identifier { get; set; }

        public object ETag { get; internal set; }

        public string CountryName { get; set; }

        public string CountryCode { get; set; } // ISO 3166 country code

        public string LanguageCodes { get; set; } // ISO 639-1 language codes

        public string OwnerTenant { get; set; }
    }
}
