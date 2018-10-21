namespace Naos.Core.App
{
    public class ServiceDescriptor // TODO: or is this pure App layer
    {
        public string Name => $"{this.Product}.{this.Capability}";

        public string Product { get; set; }

        public string Capability { get; set; }

        public string Version { get; set; } = "1.0.0";

        public string VersionInformation { get; set; } // TODO: does not belong here

        public string BuildDate { get; set; } // TODO: does not belong here + does not have the right date right now

        public override string ToString()
        {
            return $"{this.Name}-{this.Version}";
        }
    }
}
