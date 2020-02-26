namespace Naos.Sample.Countries.Application
{
    using System;
    using System.Collections.Generic;
    using Naos.Foundation.Domain;
    using Naos.Sample.Countries.Domain;

    public class CountriesExportData : IHaveCorrelationId
    {
        public string CorrelationId { get; set; }

        public DateTime Timestamp { get; set; }

        public IEnumerable<Country> Items { get; set; }
    }
}
