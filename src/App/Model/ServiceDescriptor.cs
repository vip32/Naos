namespace Naos.Core.Commands
{
    using System.Linq;

    public class ServiceDescriptor // APP layer?
    {
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
