namespace Naos.Core.Commands
{
    public class RuntimeDescriptor // APP layer?
    {
        public string Environment { get; set; }

        public string EnvironmentName { get; set; }

        public bool? IsLocal { get; set; }

        public string OperatingSystem { get; set; }

        public string ProcessArchitecture { get; set; }

        public string NetFramework { get; set; }

        public string Sitename { get; set; }

        public string Hostname { get; set; }
    }

    // howto get assembly infos (product/version) https://rehansaeed.com/optimally-configuring-asp-net-core-httpclientfactory/
}
