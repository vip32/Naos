namespace Naos.Core.ServiceDiscovery.App
{
    using System;

    public class ServiceRegistrationCheck
    {
        public string Route { get; set; } = "health";

        public TimeSpan Interval { get; set; } = new TimeSpan(0, 0, 30);

        public string CronInterval { get; set; }
    }
}
