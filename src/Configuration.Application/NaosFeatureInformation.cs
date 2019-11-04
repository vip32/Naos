namespace Naos.Configuration.Application
{
    using System.Collections.Generic;

    public class NaosFeatureInformation
    {
        public bool? Enabled { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string EchoRoute { get; set; }

        public IDictionary<string, string> SampleRoutes { get; set; }
    }
}
