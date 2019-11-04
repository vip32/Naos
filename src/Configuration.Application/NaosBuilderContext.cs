namespace Microsoft.Extensions.DependencyInjection
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Configuration;
    using Naos.Foundation;

    public class NaosBuilderContext : INaosBuilderContext
    {
        public IServiceCollection Services { get; set; }

        public Naos.Foundation.ServiceDescriptor Descriptor { get; set; }

        public string Environment { get; set; }

        public IConfiguration Configuration { get; set; }

        public List<string> Messages { get; set; } = new List<string>();

        public bool IsConsoleEnabled() => this.Configuration["console"] == "true";

        public INaosBuilderContext AddTag(string tag)
        {
            this.Descriptor ??= new Naos.Foundation.ServiceDescriptor();
            this.Descriptor.Tags = this.Descriptor.Tags.Insert(tag).ToArray();
            return this;
        }
    }
}
