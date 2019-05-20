namespace Naos.Core.Common
{
    using System;
    using System.Linq;

    public class ServiceDescriptor // APP layer?
    {
        public ServiceDescriptor(string product = null, string capability = null, string version = null, string[] tags = null)
        {
            this.Product = product ?? AppDomain.CurrentDomain.FriendlyName.SliceTillLast(".");
            this.Capability = capability ?? AppDomain.CurrentDomain.FriendlyName.SliceFromLast(".");
            this.Tags = tags;
            this.Version = version ?? "1.0.0";
        }

        public string Name => $"{this.Product}.{this.Capability}";

        public string Product { get; set; }

        public string Capability { get; set; }

        public string Version { get; set; } = "1.0.0";

        public string[] Tags { get; set; } = Enumerable.Empty<string>().ToArray();

        public override string ToString()
        {
            return $"{this.Name}-{this.Version}";
        }
    }
}
